using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Templater
{

    /// <summary>
    /// Mail service exposing static methods to send mails
    /// </summary>
    public abstract class TemplateService<TEngine> : TemplateService
    {
        public TemplateService()
            : base(TemplateMode.Standard)
        {

        }

        public TemplateService(string startExpressionToken, string startStatementToken, string endToken)
            : base(startExpressionToken, startStatementToken, endToken)
        {
        }

        public TemplateService(TemplateMode mode)
            : base(mode)
        {
        }
        public event Action<TEngine> PrepareEngine;

        public abstract TEngine CreateEngine(StringBuilder output);

        protected void RaisePrepareEngine(TEngine engine)
        {
            PrepareEngine?.Invoke(engine);
        }

        public abstract void SetParameters(TEngine engine, IEnumerable<TemplateParameter> parameters);

        public override string Process(string template, params TemplateParameter[] parameters)
        {
            return Process(new string[] { template }, parameters).FirstOrDefault();
        }

        protected abstract void Execute(TEngine engine, string script);

        public override IEnumerable<string> Process(IEnumerable<string> textsToProcess, IEnumerable<TemplateParameter> parameters = null, params TemplateParameter[] parameters2)
        {
            StringBuilder sb = new StringBuilder();
            TEngine engine = CreateEngine(sb);

            List<string> processedTexts = new List<string>();

            SetParameters(engine, parameters);
            SetParameters(engine, parameters2);

            foreach (string textToProcess in textsToProcess)
            {
                if (!string.IsNullOrWhiteSpace(textToProcess))
                {
                    Execute(engine, GenerateScriptTemplate(textToProcess));
                }

                processedTexts.Add(sb.ToString());
                sb.Clear();
            }

            return processedTexts;
        }
    }
    public abstract class TemplateService
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
            MatchCollection matches = activeTokensRegex.Matches(template);

            StringBuilder result = new StringBuilder();

            States state = States.Text;
            int index = 0;

            foreach (Match match in matches)
            {
                switch (state)
                {
                    case States.Text:
                        if (match.Index != index)
                        {
                            result.AppendFormat("write('{0}'); ", EscapeStringLiteral(template.Substring(index, match.Index - index)).Replace("\r", "\\r").Replace("\n", "\\n"));
                        }

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
                {
                    state = States.Expression;
                }
                else if (match.Value == StartStatementToken)
                {
                    state = States.Statement;
                }
                else if (match.Value == EndToken)
                {
                    state = States.Text;
                }
            }

            result.AppendFormat("write('{0}'); ", EscapeStringLiteral(template.Substring(index)).Replace("\r", "\\r").Replace("\n", "\\n"));

            return result.ToString();
        }

        private string RaiseTransformFoundScript(string p)
        {
            if (TransformFoundScript != null)
            {
                foreach (Func<string, string> handler in TransformFoundScript.GetInvocationList())
                {
                    p = handler(p);
                }
            }

            return p;
        }

        public abstract string Process(string template, params TemplateParameter[] parameters);
        public abstract IEnumerable<string> Process(IEnumerable<string> textsToProcess, IEnumerable<TemplateParameter> parameters = null, params TemplateParameter[] parameters2);

    }
}
