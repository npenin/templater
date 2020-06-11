using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Jint;
using Jint.Native;

namespace Templater
{
    /// <summary>
    /// Mail service exposing static methods to send mails
    /// </summary>
    public class TemplateService
    {
        public TemplateService()
            : this(TemplateMode.Standard)
        {

        }

        public TemplateService(string startExpressionToken, string startStatementToken, string endToken)
        {
            Initialize(startExpressionToken, startStatementToken, endToken);
        }

        public TemplateService(TemplateMode mode)
        {
            switch (mode)
            {
                case TemplateMode.Standard:
                    Initialize("<%=", "<%", "%>");
                    break;
                case TemplateMode.HtmlEscaped:
                    Initialize("&lt;%=", "&lt;%", "%&gt;");
                    break;
                default:
                    break;
            }
        }

        private void Initialize(string startExpressionToken, string startStatementToken, string endToken)
        {
            EndToken = endToken;
            StartStatementToken = startStatementToken;
            StartExpressionToken = startExpressionToken;
            activeTokensRegex = new Regex(string.Join("|", Regex.Escape(StartExpressionToken), Regex.Escape(StartStatementToken), Regex.Escape(EndToken)), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        }

        public string StartExpressionToken { get; private set; }
        public string StartStatementToken { get; private set; }
        public string EndToken { get; private set; }

        public event Func<string, string> TransformFoundScript;

        public string Process(string template, params TemplateParameter[] parameters)
        {
            return Process(new string[] { template }, parameters).FirstOrDefault();
        }

        private enum States
        {
            Text,
            Statement,
            Expression
        }

        public static string EscapeStringLiteral(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace(Environment.NewLine, "\\r\\n");
        }

        private Regex activeTokensRegex;
        /// <summary>
        /// Generates a jint script from the template
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public string GenerateScriptTemplate(string template)
        {
            var matches = activeTokensRegex.Matches(template);

            StringBuilder result = new StringBuilder();

            States state = States.Text;
            int index = 0;

            foreach (Match match in matches)
            {
                switch (state)
                {
                    case States.Text:
                        if (match.Index != index)
                            result.AppendFormat("write('{0}'); ", EscapeStringLiteral(template.Substring(index, match.Index - index)).Replace("\r", "\\r").Replace("\n", "\\n"));
                        break;
                    case States.Statement:
                        result.AppendFormat("{0}", RaiseTransformFoundScript(template.Substring(index, match.Index - index).Replace("<br />", "\n")));
                        break;
                    case States.Expression:
                        result.AppendFormat("write({0}); ", template.Substring(index, match.Index - index).Trim());
                        break;
                    default:
                        break;
                }

                index = match.Index + match.Value.Length;

                if (match.Value == StartExpressionToken)
                    state = States.Expression;
                else if (match.Value == StartStatementToken)
                    state = States.Statement;
                else if (match.Value == EndToken)
                    state = States.Text;
            }

            result.AppendFormat("write('{0}'); ", EscapeStringLiteral(template.Substring(index)).Replace("\r", "\\r").Replace("\n", "\\n"));

            return result.ToString();
        }

        private string RaiseTransformFoundScript(string p)
        {
            if (TransformFoundScript != null)
                foreach (Func<string, string> handler in TransformFoundScript.GetInvocationList())
                    p = handler(p);

            return p;
        }

        public event Action<Engine> PrepareEngine;

        private Engine CreateEngine(StringBuilder output)
        {
            var engine = new Engine();

            engine.SetValue("write", new Action<object>(s =>
            {
                if (s is TemplateParameter)
                    engine.Execute((string)((TemplateParameter)s).Value);
                else
                    output.Append(s);
            }));

            if (PrepareEngine != null)
                PrepareEngine(engine);

            return engine;
        }

        private void SetParameters(Engine engine, IEnumerable<TemplateParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (TemplateParameter parameter in parameters)
                {
                    if (parameter.IsFunction && parameter.Value != null)
                    {
                        if (parameter.IsScript)
                            engine.SetValue(parameter.Name, engine.Function.CreateFunctionObject(new Jint.Parser.Ast.FunctionDeclaration
                            {
                                Body = new Jint.Parser.Ast.BlockStatement
                                {
                                    Body = new Jint.Parser.JavaScriptParser().Parse(parameter.Value.ToString()).Body
                                }
                            }));
                        else
                            engine.SetValue(parameter.Name, (Delegate)parameter.Value);
                    }
                    else if (parameter.IsScript && parameter.Value != null)
                    {
                        parameter.Value = GenerateScriptTemplate(parameter.Value.ToString());
                        engine.SetValue(parameter.Name, parameter);
                    }
                    else
                        engine.SetValue(parameter.Name, parameter.Value);
                }
            }
        }

        public IEnumerable<string> Process(IEnumerable<string> textsToProcess, IEnumerable<TemplateParameter> parameters = null, params TemplateParameter[] parameters2)
        {
            StringBuilder sb = new StringBuilder();
            var engine = CreateEngine(sb);

            List<string> processedTexts = new List<string>();

            SetParameters(engine, parameters);
            SetParameters(engine, parameters2);

            foreach (var textToProcess in textsToProcess)
            {
                if (!string.IsNullOrWhiteSpace(textToProcess))
                    engine.Execute(GenerateScriptTemplate(textToProcess));
                processedTexts.Add(sb.ToString());
                sb.Clear();
            }

            return processedTexts;
        }
    }
}
