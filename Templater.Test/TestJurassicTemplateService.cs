using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web;

namespace Templater.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestJurassicTemplateService
    {
        public TestJurassicTemplateService()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get => testContextInstance;
            set => testContextInstance = value;
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        public static TemplateJurassicService Create()
        {
            return new TemplateJurassicService();
        }

        public static TemplateJurassicService Create(string startExpressionToken, string startStatementToken, string endToken)
        {
            return new TemplateJurassicService(startExpressionToken, startStatementToken, endToken);
        }

        public static TemplateJurassicService Create(TemplateMode mode)
        {
            return new TemplateJurassicService(mode);
        }


        [TestMethod]
        public void TestBasicUsage()
        {
            Assert.AreEqual("pwet", Create().Process("<%= pwic %>", new TemplateParameter("pwic", "pwet")));
            Assert.AreEqual("pwet", Create(TemplateMode.HtmlEscaped).Process("&lt;%= pwic %&gt;", new TemplateParameter("pwic", "pwet")));
        }

        [TestMethod]
        public void TestScriptTransformation()
        {
            TemplateJurassicService templateService = Create(TemplateMode.HtmlEscaped);
            templateService.TransformFoundScript += new Func<string, string>(templateService_TransformFoundScript);
            Assert.AreEqual("pwet", templateService.Process("&lt;% if(1&lt;2) write(&quot;pwet&quot;); %&gt;"));
            //Assert.AreEqual("pwet", templateService.Process("&lt;% if(1&lt;2) %&lt; &lt;%= 'pwet' %&gt;"));
            templateService = Create(TemplateMode.Standard);
            templateService.TransformFoundScript += new Func<string, string>(templateService_TransformFoundScript);
            Assert.AreEqual("pwet", templateService.Process("<% if(2>1) { %><%= 'pwet' %><% } %><% else if(2>1) { %><%= 'pwic' %><% } %>"));
            Assert.AreEqual("pwic", templateService.Process("<% if(1>2) { %><%= 'pwet' %><% } %><% else if(2>1) { %><%= 'pwic' %><% } %>"));
        }

        [TestMethod]
        public void TestEnginePreparation()
        {
            TemplateJurassicService templateService = Create();
            templateService.PrepareEngine += new System.Action<Jurassic.ScriptEngine>(templateService_PrepareEngine);
            Assert.AreEqual("pwet", templateService.Process("<% write('pwet'); %>"));
            templateService = Create(TemplateMode.HtmlEscaped);
            templateService.PrepareEngine += new System.Action<Jurassic.ScriptEngine>(templateService_PrepareEngine);
            Assert.AreEqual("pwet", templateService.Process("&lt;% write2('pwet'); %&gt;"));
        }

        private void templateService_PrepareEngine(Jurassic.ScriptEngine engine)
        {
            engine.SetGlobalFunction("write2", new Action<string>(s => engine.GetGlobalValue<Jurassic.Library.FunctionInstance>("write").Call(engine.Global, s)));
        }

        private string templateService_TransformFoundScript(string arg)
        {
            return HttpUtility.HtmlDecode(arg);
        }
    }
}
