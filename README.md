# ExpressionTree

A C# implementation of an infix expression parser. This library converts from a text representation to RPN and then to a tree form. The tree can then be evaluated. The underlying algorithm used is the [Shunting Yard algorithm](https://en.wikipedia.org/wiki/Shunting-yard_algorithm). 

This is meant to be a very small implementation. For fancier use cases, if you have access to the .NET framework, you're better off using C#'s built in Expressions library. 

Some features:
- All the basic binary infix operators (e.g. +, -, *, /)
- Parenthezation
- Logical operators and tests (equality, greater than)
- Correct and expected precidence for all operators and functions
- Unary operators for negative numbers and expressions
- User-defined functions (currently requries modifying library source)
- User-defined variables and hashtables
- Some amount of conversion between operand types (e.g. logical -> int, string)
