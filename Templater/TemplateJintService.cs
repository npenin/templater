using Jint;
using System;
using System.Collections.Generic;
using System.Text;

namespace Templater
{
    /// <summary>
    /// Mail service exposing static methods to send mails
    /// </summary>
    public class TemplateJintService : TemplateService<Engine>
    {
        public TemplateJintService()
            : base(TemplateMode.Standard)
        {

        }

        public TemplateJintService(string startExpressionToken, string startStatementToken, string endToken)
            : base(startExpressionToken, startStatementToken, endToken)
        {
        }

        public TemplateJintService(TemplateMode mode)
            : base(mode)
        {
        }


        public override Engine CreateEngine(StringBuilder output)
        {
            Engine engine = new Engine();

            engine.SetValue("write", new Action<object>(s =>
            {
                if (s is TemplateParameter)
                {
                    engine.Execute((string)((TemplateParameter)s).Value);
                }
                else
                {
                    output.Append(s);
                }
            }));

            RaisePrepareEngine(engine);

            return engine;
        }

        public override void SetParameters(Engine engine, IEnumerable<TemplateParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (TemplateParameter parameter in parameters)
                {
                    if (parameter.IsFunction && parameter.Value != null)
                    {
                        if (parameter.IsScript)
                        {
                            engine.SetValue(parameter.Name, engine.Function.CreateFunctionObject(new Jint.Parser.Ast.FunctionDeclaration
                            {
                                Body = new Jint.Parser.Ast.BlockStatement
                                {
                                    Body = new Jint.Parser.JavaScriptParser().Parse(parameter.Value.ToString()).Body
                                }
                            }));
                        }
                        else
                        {
                            engine.SetValue(parameter.Name, (Delegate)parameter.Value);
                        }
                    }
                    else if (parameter.IsScript && parameter.Value != null)
                    {
                        parameter.Value = GenerateScriptTemplate(parameter.Value.ToString());
                        engine.SetValue(parameter.Name, parameter);
                    }
                    else
                    {
                        engine.SetValue(parameter.Name, parameter.Value);
                    }
                }
            }
        }

        protected override void Execute(Engine engine, string script)
        {
            engine.Execute(script);
        }
    }
}
