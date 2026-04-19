using System;
using System.Collections.Generic;
using System.Linq;
using compiles_lab_1.Core.Ast;

namespace compiles_lab_1.Core
{
    public class SemanticResult
    {
        public List<AstNode> AstNodes { get; } = new();
        public List<SemanticError> Errors { get; } = new();
    }

    public static class SemanticAnalyzer
    {
        private const long IntMin = -2147483648;
        private const long IntMax = 2147483647;

        public static SemanticResult Analyze(string source)
        {
            var result = new SemanticResult();
            var symbolTable = new SymbolTable();

            var scan = Scanner.Analyze(source);
            var tokens = scan.Lexemes;

            var statements = SplitBySemicolon(tokens);

            foreach (var stmt in statements)
                TryProcessConstDecl(stmt, symbolTable, result);

            return result;
        }

        private static List<List<Lexeme>> SplitBySemicolon(List<Lexeme> tokens)
        {
            var result = new List<List<Lexeme>>();
            var current = new List<Lexeme>();

            foreach (var t in tokens)
            {
                current.Add(t);

                if (t.Code == LexemeCode.Semicolon)
                {
                    result.Add(current);
                    current = new List<Lexeme>();
                }
            }

            return result;
        }

        private static void TryProcessConstDecl(
            List<Lexeme> tokens,
            SymbolTable symbols,
            SemanticResult result)
        {
            if (tokens.Count < 7) return;

            if (tokens[0].Code != LexemeCode.KeywordConst) return;
            if (tokens[1].Code != LexemeCode.KeywordVal) return;
            if (tokens[2].Code != LexemeCode.Identifier) return;
            if (tokens[3].Code != LexemeCode.Colon) return;
            if (tokens[4].Code != LexemeCode.KeywordInt) return;
            if (tokens[5].Code != LexemeCode.Assign) return;
            if (tokens[^1].Code != LexemeCode.Semicolon) return;

            var idTok = tokens[2];
            string name = idTok.Text;

            if (!symbols.Declare(new SymbolInfo
            {
                Name = name,
                TypeName = "Int",
                Value = 0,
                Line = idTok.Line
            }, out var existing))
            {
                result.Errors.Add(new SemanticError
                {
                    Line = idTok.Line,
                    StartColumn = idTok.StartColumn,
                    EndColumn = idTok.EndColumn,
                    Message = $"Ошибка: идентификатор \"{name}\" уже объявлен ранее (строка {existing.Line})"
                });
                return;
            }

            var rhsTokens = tokens
                .Skip(6)
                .Take(tokens.Count - 7)
                .ToList();

            string rhsText = string.Join("", rhsTokens.Select(t => t.Text));

            if (rhsTokens.Count == 0)
            {
                result.Errors.Add(new SemanticError
                {
                    Line = idTok.Line,
                    StartColumn = tokens[5].EndColumn + 1,
                    EndColumn = tokens[^1].StartColumn - 1,
                    Message = "Ошибка: ожидалось число типа Int, встретилось пусто"
                });
                return;
            }

            if (rhsTokens.Any(t => t.Code == LexemeCode.Error))
            {
                result.Errors.Add(new SemanticError
                {
                    Line = rhsTokens[0].Line,
                    StartColumn = rhsTokens[0].StartColumn,
                    EndColumn = rhsTokens[^1].EndColumn,
                    Message = $"Ошибка: ожидалось число типа Int, встретилось {rhsText}"
                });
                return;
            }

            bool isNegative = rhsTokens.Count >= 2 &&
                              rhsTokens[0].Code == LexemeCode.Minus &&
                              rhsTokens[1].Code == LexemeCode.Integer;

            long parsedValue;

            if (isNegative)
            {
                if (!long.TryParse("-" + rhsTokens[1].Text, out parsedValue))
                {
                    result.Errors.Add(new SemanticError
                    {
                        Line = rhsTokens[0].Line,
                        StartColumn = rhsTokens[0].StartColumn,
                        EndColumn = rhsTokens[1].EndColumn,
                        Message = $"Ошибка: ожидалось число типа Int, встретилось {rhsText}"
                    });
                    return;
                }
            }
            else if (rhsTokens.Count == 1 && rhsTokens[0].Code == LexemeCode.Integer)
            {
                if (!long.TryParse(rhsTokens[0].Text, out parsedValue))
                {
                    result.Errors.Add(new SemanticError
                    {
                        Line = rhsTokens[0].Line,
                        StartColumn = rhsTokens[0].StartColumn,
                        EndColumn = rhsTokens[0].EndColumn,
                        Message = $"Ошибка: ожидалось число типа Int, встретилось {rhsText}"
                    });
                    return;
                }
            }
            else
            {
                result.Errors.Add(new SemanticError
                {
                    Line = rhsTokens[0].Line,
                    StartColumn = rhsTokens[0].StartColumn,
                    EndColumn = rhsTokens[^1].EndColumn,
                    Message = $"Ошибка: ожидалось число типа Int, встретилось {rhsText}"
                });
                return;
            }

            if (parsedValue < IntMin || parsedValue > IntMax)
            {
                result.Errors.Add(new SemanticError
                {
                    Line = rhsTokens[0].Line,
                    StartColumn = rhsTokens[0].StartColumn,
                    EndColumn = rhsTokens[^1].EndColumn,
                    Message = $"Ошибка: ожидалось число от {IntMin} до {IntMax}"
                });
                return;
            }


            var node = new ConstDeclNode
            {
                Line = idTok.Line,
                StartColumn = tokens[0].StartColumn,
                EndColumn = tokens[^1].EndColumn,
                Name = name,
                Modifiers = new[] { "const", "val" }
            };
 
            var mods = new ModifiersNode();
            mods.Children.Add(new TokenNode("const"));
            mods.Children.Add(new TokenNode("val"));
            node.Children.Add(mods);
 
            var idNode = new IdentifierNode { Name = name };
            idNode.Children.Add(new TokenNode(name));
            idNode.Children.Add(new TokenNode(":")); 
            node.Children.Add(idNode);
 
            var typeNode = new IntNode { Name = "Int" };
            typeNode.Children.Add(new TokenNode("Int"));
            typeNode.Children.Add(new TokenNode("="));  
            node.Children.Add(typeNode);
 
            var valNode = new IntLiteralNode { Value = (int)parsedValue };
            valNode.Children.Add(new TokenNode(parsedValue.ToString()));
            node.Children.Add(valNode);

            var semi = new SemicolonNode();
            semi.Children.Add(new TokenNode(";"));
            node.Children.Add(semi);

            result.AstNodes.Add(node);


        }
    }
}
