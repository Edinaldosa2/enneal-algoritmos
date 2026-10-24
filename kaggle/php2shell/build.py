import nbformat as nbf
nb=nbf.v4.new_notebook(); C=[]
md=lambda s:C.append(nbf.v4.new_markdown_cell(s)); code=lambda s:C.append(nbf.v4.new_code_cell(s))
md("""# Hunting PHP Webshell Patterns in Upload Payloads and Access Logs (PHP2Shell context)

*By Edinaldo Sá, Enneal | defensive notebook, runs offline*

In October 2026 the PHP project fixed CVE-2026-109618 and CVE-2026-109619 (called "php2shell" by CERT Polska). **The real fix is updating PHP** (8.5.12, 8.4.27, 8.3.36, 8.2.35 or your distro's patched package). This notebook does not reproduce those flaws. It covers a related, everyday defensive task: spotting signs that someone is trying to turn a file upload into a PHP shell, or is calling one.

**All data below is synthetic, written by hand for this notebook. It is not taken from any real system or incident.** The rules are simple heuristics: useful for triage, not a replacement for patching, a WAF or a proper EDR. Expect false positives and false negatives.""")
md("## 1. Synthetic sample data\nUploads (filename + first bytes of content) and access-log lines. Labels are ours, for evaluating the rules.")
code('''import re, base64, pandas as pd
# SYNTHETIC EXAMPLES - illustrative only
uploads = [
 ("avatar.png",        b"\\x89PNG\\r\\n\\x1a\\n....IHDR", "benign"),
 ("report.pdf",        b"%PDF-1.7 ...", "benign"),
 ("photo.jpg.php",     b"\\xff\\xd8\\xff\\xe0<?php system($_GET['c']); ?>", "malicious"),
 ("shell.phtml",       b"<?php eval(base64_decode($_POST['x'])); ?>", "malicious"),
 ("gif89.gif",         b"GIF89a<?php echo shell_exec($_REQUEST['q']); ?>", "malicious"),
 (".htaccess",         b"AddType application/x-httpd-php .jpg", "malicious"),
 ("notes.txt",         b"Meeting notes: discuss php upgrade to 8.4.27", "benign"),
 ("short.php5",        b"<?=`$_GET[1]`?>", "malicious"),
 ("readme.md",         b"Use <?php echo 'hello'; ?> in templates", "benign"),
]
up = pd.DataFrame(uploads, columns=["filename","content","label"])
logs = [
 '203.0.113.10 - - [22/Oct/2026:10:01:02 +0000] "GET /index.php HTTP/1.1" 200 5120',
 '203.0.113.11 - - [22/Oct/2026:10:01:05 +0000] "POST /upload.php HTTP/1.1" 200 88',
 '198.51.100.7 - - [22/Oct/2026:10:02:10 +0000] "GET /uploads/photo.jpg.php?c=id HTTP/1.1" 200 41',
 '198.51.100.7 - - [22/Oct/2026:10:02:15 +0000] "GET /uploads/x.php?cmd=cat%20/etc/passwd HTTP/1.1" 200 900',
 '192.0.2.5 - - [22/Oct/2026:10:03:00 +0000] "GET /wp-content/uploads/2026/10/a.phtml HTTP/1.1" 200 12',
 '203.0.113.12 - - [22/Oct/2026:10:04:00 +0000] "GET /blog/?s=php+tutorial HTTP/1.1" 200 7000',
 '192.0.2.9 - - [22/Oct/2026:10:05:00 +0000] "GET /index.php?page=../../../../etc/passwd HTTP/1.1" 200 1500',
]
lg = pd.DataFrame({"line":logs, "label":["benign","benign","malicious","malicious","malicious","benign","malicious"]})
up[["filename","label"]]''')
md("## 2. Upload heuristics\nScore each file: dangerous extension anywhere in the name, PHP open tags, dangerous functions with user input, magic-byte/extension mismatch, `.htaccess` handler tricks.")
code('''DANGEROUS_EXT = re.compile(r"\\.(php\\d?|phtml|phar|pht|phps|inc)(\\.|$)", re.I)
PHP_TAG = re.compile(rb"<\\?(php|=)", re.I)
DANGER_FUNC = re.compile(rb"\\b(eval|assert|system|exec|shell_exec|passthru|popen|proc_open)\\s*\\(|`[^`]*\\$_", re.I)
USER_INPUT = re.compile(rb"\\$_(GET|POST|REQUEST|COOKIE|FILES|SERVER)", re.I)
OBFUSC = re.compile(rb"base64_decode|gzinflate|str_rot13|\\\\x[0-9a-f]{2}", re.I)
HTACCESS = re.compile(rb"(AddType|AddHandler|SetHandler).*php", re.I)
MAGIC = {b"\\x89PNG":"png", b"\\xff\\xd8\\xff":"jpg", b"GIF8":"gif", b"%PDF":"pdf"}

def score_upload(name, content):
    reasons=[]
    if DANGEROUS_EXT.search(name): reasons.append("php-like extension")
    if name.lower()==".htaccess" and HTACCESS.search(content): reasons.append("htaccess handler override")
    if PHP_TAG.search(content): reasons.append("php open tag")
    if DANGER_FUNC.search(content): reasons.append("exec/eval call")
    if USER_INPUT.search(content): reasons.append("user input superglobal")
    if OBFUSC.search(content): reasons.append("obfuscation")
    if any(content.startswith(m) for m in MAGIC) and PHP_TAG.search(content): reasons.append("image/pdf header + php (polyglot)")
    weights={"php-like extension":2,"htaccess handler override":3,"php open tag":1,"exec/eval call":2,
             "user input superglobal":2,"obfuscation":1,"image/pdf header + php (polyglot)":2}
    return sum(weights[r] for r in reasons), reasons

up[["score","reasons"]] = up.apply(lambda r: pd.Series(score_upload(r.filename, r.content)), axis=1)
up["flag"] = up.score >= 3
up[["filename","label","score","flag","reasons"]]''')
md("Note `readme.md`: it contains a PHP tag but no execution or user input, so it stays below the threshold. Tuning the threshold is a trade-off you should measure on your own (non-synthetic) data.")
md("## 3. Access-log heuristics\nLook for requests to PHP files inside upload folders, command-like parameters, and path traversal.")
code('''LOG_RE = re.compile(r'^(?P<ip>\\S+) \\S+ \\S+ \\[(?P<ts>[^\\]]+)\\] "(?P<method>\\S+) (?P<path>\\S+) [^"]*" (?P<status>\\d{3}) (?P<size>\\d+)')
from urllib.parse import unquote
RULES = {
 "php in uploads dir": re.compile(r"/(uploads?|wp-content/uploads|files|media)/[^?]*\\.(php\\d?|phtml|phar)", re.I),
 "command parameter":  re.compile(r"[?&](c|cmd|exec|command|q)=[^&]*(id|whoami|uname|cat|wget|curl|/etc/passwd)", re.I),
 "path traversal":     re.compile(r"\\.\\./"),
}
def analyse(line):
    m = LOG_RE.match(line); d = m.groupdict()
    p = unquote(d["path"])
    d["hits"] = [k for k,rx in RULES.items() if rx.search(p)]
    return d
parsed = pd.DataFrame([analyse(l) for l in lg.line])
parsed["label"] = lg.label
parsed["flag"] = parsed.hits.str.len() > 0
parsed[["ip","method","path","status","hits","flag","label"]]''')
md("## 4. How did the rules do on this synthetic set?\nThis only checks the rules against the examples we wrote ourselves; it says nothing about real-world accuracy.")
code('''def confusion(df):
    return pd.crosstab(df.label, df.flag, rownames=["label"], colnames=["flagged"])
print("Uploads"); display(confusion(up))
print("Logs");    display(confusion(parsed))
print("IPs with any flagged request:", sorted(parsed[parsed.flag].ip.unique()))''')
md("""## 5. Turning this into practice
1. **Patch PHP first.** Heuristics do not stop an engine-level flaw that fires before application code runs.
2. Store uploads outside the web root, rename them, and never let the upload folder execute PHP.
3. Allow-list extensions and check magic bytes server-side.
4. Feed rules like these into your log pipeline as *alerts for human review*, not automatic blocks.
5. For affected third-party software, see the NCSC-NL list: github.com/NCSC-NL/php2shell.

Sources for the CVE context: PHP project announcement, CERT-EU advisory 2026-018, CERT-FR CERTFR-2026-ALE-014, CERT Polska notice 196/2026.

---
Made by **Edinaldo Sá / Enneal**. [enneal.com.br](https://enneal.com.br) | [LinkedIn](https://www.linkedin.com/in/edinaldosa/)""")
nb.cells=C
nb.metadata["kernelspec"]={"name":"python3","display_name":"Python 3","language":"python"}
nbf.write(nb,"php2shell-webshell-detection.ipynb")
