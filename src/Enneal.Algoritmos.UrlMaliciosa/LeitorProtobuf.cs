using System.Buffers.Binary;
using System.Text;

namespace Enneal.Algoritmos.UrlMaliciosa;

/// <summary>
/// Um leitor mínimo do formato protobuf (o formato binário do arquivo .onnx). Só o necessário para ler o
/// modelo de árvores: varints, números de 32 bits e blocos com tamanho.
/// </summary>
internal sealed class LeitorProtobuf
{
    private readonly byte[] _dados;
    private int _pos;
    private readonly int _fim;

    public LeitorProtobuf(byte[] dados, int inicio, int fim)
    {
        _dados = dados;
        _pos = inicio;
        _fim = fim;
    }

    public bool Acabou => _pos >= _fim;

    // Cada campo começa com uma "etiqueta": número do campo << 3 | tipo de codificação.
    public (int Campo, int Tipo) Etiqueta()
    {
        ulong e = Varint();
        return ((int)(e >> 3), (int)(e & 7));
    }

    public ulong Varint()
    {
        ulong v = 0;
        for (int desloc = 0; desloc < 64; desloc += 7)
        {
            byte b = _dados[_pos++];
            v |= (ulong)(b & 0x7F) << desloc;
            if ((b & 0x80) == 0) return v;
        }
        throw new InvalidDataException("Varint grande demais.");
    }

    public float Float32()
    {
        float f = BinaryPrimitives.ReadSingleLittleEndian(_dados.AsSpan(_pos, 4));
        _pos += 4;
        return f;
    }

    // Um bloco com tamanho (tipo 2): devolve um leitor só para ele.
    public LeitorProtobuf Bloco()
    {
        int tamanho = checked((int)Varint());
        var sub = new LeitorProtobuf(_dados, _pos, _pos + tamanho);
        _pos += tamanho;
        return sub;
    }

    public string Texto()
    {
        var b = Bloco();
        return Encoding.UTF8.GetString(_dados, b._pos, b._fim - b._pos);
    }

    public void Pular(int tipo)
    {
        switch (tipo)
        {
            case 0: Varint(); break;
            case 1: _pos += 8; break;
            case 2:
                int tamanho = checked((int)Varint()); // lê o tamanho ANTES de somar: Varint() anda o _pos
                _pos += tamanho;
                break;
            case 5: _pos += 4; break;
            default: throw new InvalidDataException($"Tipo protobuf {tipo} não suportado.");
        }
    }
}
