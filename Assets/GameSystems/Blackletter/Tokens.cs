
////TODO: separate file (?)


//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;

//namespace Blackletter
//{
//#nullable enable
//    public readonly struct TextSpan : IEquatable<TextSpan>
//    {
//        public readonly int Start;
//        public readonly int Length;

//        public int End => Start + Length; // exclusive

//        public TextSpan(int start, int length)
//        {
//            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
//            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
//            Start = start;
//            Length = length;
//        }

//        public bool Contains(int position) => (uint)(position - Start) < (uint)Length;
//        public bool Intersects(TextSpan other) => Start < other.End && other.Start < End;

//        public static TextSpan FromBounds(int startInclusive, int endExclusive)
//        {
//            if (endExclusive < startInclusive) throw new ArgumentOutOfRangeException(nameof(endExclusive));
//            return new TextSpan(startInclusive, endExclusive - startInclusive);
//        }

//        public override string ToString() => $"[{Start}..{End})";

//        public bool Equals(TextSpan other) => Start == other.Start && Length == other.Length;
//        public override bool Equals(object? obj) => obj is TextSpan other && Equals(other);
//        public override int GetHashCode() => HashCode.Combine(Start, Length);

//        public static bool operator ==(TextSpan a, TextSpan b) => a.Equals(b);
//        public static bool operator !=(TextSpan a, TextSpan b) => !a.Equals(b);
//    }


//    public readonly struct LinePosition : IEquatable<LinePosition>
//    {
//        public readonly int Line;   // 1-based
//        public readonly int Column; // 1-based

//        public LinePosition(int line, int column)
//        {
//            Line = line < 1 ? 1 : line;
//            Column = column < 1 ? 1 : column;
//        }

//        public override string ToString() => $"{Line}:{Column}";
//        public bool Equals(LinePosition other) => Line == other.Line && Column == other.Column;
//        public override bool Equals(object? obj) => obj is LinePosition other && Equals(other);
//        public override int GetHashCode() => HashCode.Combine(Line, Column);
//        public static bool operator ==(LinePosition a, LinePosition b) => a.Equals(b);
//        public static bool operator !=(LinePosition a, LinePosition b) => !a.Equals(b);
//    }


//    public sealed class LineMap
//    {
//        // Stores the starting offset of each line.
//        // lineStarts[0] = 0 always.
//        private readonly int[] _lineStarts;

//        public LineMap(string source)
//        {
//            if (source == null) throw new ArgumentNullException(nameof(source));

//            var starts = new List<int>(capacity: 128) { 0 };
//            for (int i = 0; i < source.Length; i++)
//            {
//                char c = source[i];
//                if (c == '\n')
//                {
//                    int next = i + 1;
//                    if (next <= source.Length) starts.Add(next);
//                }
//            }

//            _lineStarts = starts.ToArray();
//        }

//        /// <summary>Returns 1-based line/col for an absolute position (clamped).</summary>
//        public LinePosition GetLinePosition(int absolutePosition)
//        {
//            if (_lineStarts.Length == 0) return new LinePosition(1, 1);

//            if (absolutePosition < 0) absolutePosition = 0;
//            // allow pointing at EOF
//            // caller may pass source.Length, which maps to last line column+1.
//            // binary search for rightmost line start <= position
//            int idx = Array.BinarySearch(_lineStarts, absolutePosition);
//            if (idx < 0) idx = ~idx - 1;
//            if (idx < 0) idx = 0;

//            int lineStart = _lineStarts[idx];
//            int line = idx + 1; // 1-based
//            int col = (absolutePosition - lineStart) + 1; // 1-based
//            return new LinePosition(line, col);
//        }
//    }

//    // ----------------------------
//    // Token model
//    // ----------------------------

//    public enum TokenKind : ushort
//    {
//        // Special / control
//        Bad = 0,
//        EndOfFile,

//        // Layout (important for indentation languages)
//        Newline,   // '\n' (or normalized line break)
//        Indent,    // produced by lexer after newline when indent increases
//        Dedent,    // produced by lexer when indent decreases
//        Whitespace, // optional (usually you won't emit these in the main stream)
//        Comment,    // optional

//        // Identifiers / literals
//        Identifier,
//        Integer,
//        Float,
//        String,


//        If,
//        Elif,
//        Else,
//        While,
//        For,
//        In,
//        Def,
//        Return,
//        Break,
//        Continue,
//        True,
//        False,
//        None,


//        Plus,          // +
//        Minus,         // -
//        Star,          // *
//        Slash,         // /
//        Percent,       // %
//        Caret,         // ^
//        Ampersand,     // &
//        Pipe,          // |
//        Tilde,         // ~
//        Bang,          // !
//        Question,      // ?
//        Colon,         // :
//        Comma,         // ,
//        Dot,           // .
//        Semicolon,     // ;
//        At,            // @
//        Equal,         // =
//        Less,          // <
//        Greater,       // >
//        LParen,        // (
//        RParen,        // )
//        LBracket,      // [
//        RBracket,      // ]
//        LBrace,        // {
//        RBrace,        // }

//        // Multi-char operators
//        EqualEqual,    // ==
//        BangEqual,     // !=
//        LessEqual,     // <=
//        GreaterEqual,  // >=
//        Arrow,         // ->
//        PlusEqual,     // +=
//        MinusEqual,    // -=
//        StarEqual,     // *=
//        SlashEqual,    // /=
//        AndAnd,        // &&
//        OrOr,          // ||
//    }

//    public enum TokenFlags : byte
//    {
//        None = 0,
//        Synthetic = 1 << 0, // produced by lexer (INDENT/DEDENT), not present in source
//    }



//    public readonly struct Token : IEquatable<Token>
//    {
//        //TODO add meta for rendering


//        public readonly TokenKind Kind;
//        public readonly TextSpan Span;
//        public readonly TokenFlags Flags;

//        // Optional "pre-parsed" payload:
//        // - Integer: long
//        // - Float: double
//        // - String: string (already unescaped, or raw—your choice)
//        // - Identifier: often null (use Span to slice from source), or interned id if you add one later.
//        public readonly object? Value;

//        public Token(TokenKind kind, TextSpan span, object? value = null, TokenFlags flags = TokenFlags.None)
//        {
//            Kind = kind;
//            Span = span;
//            Value = value;
//            Flags = flags;
//        }

//        public bool IsSynthetic => (Flags & TokenFlags.Synthetic) != 0;

//        public override string ToString()
//        {
//            // Avoid printing Value unless present; keeps logs tidy.
//            return Value is null
//                ? $"{Kind} {Span}"
//                : $"{Kind} {Span} = {Value}";
//        }

//        public bool Equals(Token other)
//            => Kind == other.Kind && Span == other.Span && Flags == other.Flags && Equals(Value, other.Value);

//        public override bool Equals(object? obj) => obj is Token other && Equals(other);
//        public override int GetHashCode() => HashCode.Combine((int)Kind, Span, (int)Flags, Value);

//        public static bool operator ==(Token a, Token b) => a.Equals(b);
//        public static bool operator !=(Token a, Token b) => !a.Equals(b);
//    }

//    public static class TokenText
//    {
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static string Slice(string source, TextSpan span)
//        {
//            if (source == null) throw new ArgumentNullException(nameof(source));
//            if ((uint)span.Start > (uint)source.Length) return string.Empty;

//            int start = span.Start;
//            int len = span.Length;

//            if (start + len > source.Length) len = Math.Max(0, source.Length - start);
//            if (len <= 0) return string.Empty;

//            return source.Substring(start, len);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static ReadOnlySpan<char> SliceSpan(string source, TextSpan span)
//        {
//            if (source == null) throw new ArgumentNullException(nameof(source));
//            int start = Math.Clamp(span.Start, 0, source.Length);
//            int end = Math.Clamp(span.End, 0, source.Length);
//            if (end <= start) return ReadOnlySpan<char>.Empty;
//            return source.AsSpan(start, end - start);
//        }
//    }

//    public enum DiagnosticSeverity : byte
//    {
//        Info,
//        Warning,
//        Error
//    }

//    public readonly struct Diagnostic : IEquatable<Diagnostic>
//    {
//        public readonly DiagnosticSeverity Severity;
//        public readonly string Message;
//        public readonly TextSpan Span;
//        public readonly string? Code; 

//        public Diagnostic(DiagnosticSeverity severity, string message, TextSpan span, string? code = null)
//        {
//            Severity = severity;
//            Message = message ?? "";
//            Span = span;
//            Code = code;
//        }

//        public override string ToString()
//            => Code is null
//                ? $"{Severity}: {Message} @ {Span}"
//                : $"{Severity} {Code}: {Message} @ {Span}";

//        public bool Equals(Diagnostic other)
//            => Severity == other.Severity && Message == other.Message && Span == other.Span && Code == other.Code;

//        public override bool Equals(object? obj) => obj is Diagnostic other && Equals(other);
//        public override int GetHashCode() => HashCode.Combine((int)Severity, Message, Span, Code);

//        public static bool operator ==(Diagnostic a, Diagnostic b) => a.Equals(b);
//        public static bool operator !=(Diagnostic a, Diagnostic b) => !a.Equals(b);
//    }

//    public sealed class DiagnosticBag
//    {
//        private readonly List<Diagnostic> _items = new List<Diagnostic>(capacity: 16);
//        public IReadOnlyList<Diagnostic> Items => _items;

//        public void Clear() => _items.Clear();

//        public void Add(DiagnosticSeverity severity, string message, TextSpan span, string? code = null)
//            => _items.Add(new Diagnostic(severity, message, span, code));

//        public void Error(string message, TextSpan span, string? code = null)
//            => Add(DiagnosticSeverity.Error, message, span, code);

//        public void Warning(string message, TextSpan span, string? code = null)
//            => Add(DiagnosticSeverity.Warning, message, span, code);

//        public void Info(string message, TextSpan span, string? code = null)
//            => Add(DiagnosticSeverity.Info, message, span, code);
//    }

//    public readonly struct TokenReader
//    {
//        private readonly IReadOnlyList<Token> _tokens;
//        public readonly int Position;

//        public TokenReader(IReadOnlyList<Token> tokens, int position = 0)
//        {
//            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
//            Position = Math.Clamp(position, 0, tokens.Count);
//        }

//        public int Count => _tokens.Count;

//        public Token Current => Peek(0);

//        public Token Peek(int offset)
//        {
//            int index = Position + offset;
//            if ((uint)index >= (uint)_tokens.Count)
//            {
//                return new Token(TokenKind.EndOfFile, new TextSpan(_tokens.Count > 0 ? _tokens[_tokens.Count - 1].Span.End : 0, 0), flags: TokenFlags.Synthetic);
//            }

//            return _tokens[index];
//        }

//        public TokenReader Advance(int count = 1)
//        {
//            int next = Math.Clamp(Position + count, 0, _tokens.Count);
//            return new TokenReader(_tokens, next);
//        }

//        public bool IsAtEnd => Current.Kind == TokenKind.EndOfFile;
//    }
//    public static class Keywords
//    {
//        private static readonly Dictionary<string, TokenKind> _map = new Dictionary<string, TokenKind>(StringComparer.Ordinal)
//        {
//            ["if"] = TokenKind.If,
//            ["elif"] = TokenKind.Elif,
//            ["else"] = TokenKind.Else,
//            ["while"] = TokenKind.While,
//            ["for"] = TokenKind.For,
//            ["in"] = TokenKind.In,
//            ["def"] = TokenKind.Def,
//            ["return"] = TokenKind.Return,
//            ["break"] = TokenKind.Break,
//            ["continue"] = TokenKind.Continue,
//            ["true"] = TokenKind.True,
//            ["false"] = TokenKind.False,
//            ["none"] = TokenKind.None,
//        };

//        public static bool TryGetKeywordKind(ReadOnlySpan<char> text, out TokenKind kind)
//        {
//            //avoid allocations: only allocate if we must.
//            if (text.Length == 0)
//            {
//                kind = TokenKind.Identifier;
//                return false;
//            }

//            string s = text.ToString();
//            if (_map.TryGetValue(s, out kind))
//                return true;

//            kind = TokenKind.Identifier;
//            return false;
//        }
//    }
//#nullable disable
//}
