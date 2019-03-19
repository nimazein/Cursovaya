using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;


namespace CursovayaWF
{
    public partial class Form1 : Form
    {            
        private FileStream WrongStringSerializator = new FileStream("WrongLines.bin", FileMode.Create);
        private BinaryFormatter WrongStringBSerializator = new BinaryFormatter();

        private FileStream ErrorMessageSerializator = new FileStream("ErrorMessages.bin", FileMode.Create);
        private BinaryFormatter ErrorMessageBSerializator = new BinaryFormatter();

        private double NumberOfStrings;
        private double NumberOfCommentedStrings;
         
        private Regex NonConstantFieldNamingTemplate = new Regex(@".*\W*[(sbyte)(byte)(short)(ushort)(int)(uint)(long)(ulong)(char)(float)(double)(decimal)(bool)]/s{1}[A-Z].*\W*");
       
        private Regex LocalVarNamingTemplate = new Regex(@".*\W*[(sbyte)(byte)(short)(ushort)(int)(uint)(long)(ulong)(char)(float)(double)(decimal)(bool)]/s{1}[a-z].*\W*");
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = GetFilePath();
            StreamReader CodeReader = new StreamReader(filePath);
            
            while (!CodeReader.EndOfStream)
            {
                string currentString = CodeReader.ReadLine();
                if (!IsEmpty(currentString))
                {
                    NumberOfStrings++;
                    CheckCurrentString(currentString);
                }
            }
            CheckPercentageOfComments();
            CodeReader.Close();

            ErrorMessageSerializator.Close();
            WrongStringSerializator.Close();

            if (NoMistakes())
            {
                string message = "Ошибок оформления в файле не обнаружено";
                MessageBox.Show(message);

                Application.Exit();
            }

            Output output = new Output();
            output.ShowResult();
            output.ShowDialog();
         
        }
        void CheckCurrentString(string currentString)
        {
            CheckStringLength(currentString);
            if (IsCommented(currentString))
            {
                NumberOfCommentedStrings++;
                return;
            }
            if (IsEssenceOutsideMethods(currentString))
            {
                CheckEssencesOutsideMethods(currentString);
            }     
            if (IsConst(currentString))
            {
                CheckConstNaming(currentString);
                return;
            }
            if (ContainsForbiddenWords(currentString))
            {
                CheckForForbiddenWords(currentString);
            }
            if (ContainsMethodSignature(currentString))
            {
                CheckMethodSignature(currentString);
                return;
            }
            if (ContainsVar(currentString))
            {
                CheckVarNaming(currentString);
            }            
        }

        void CheckStringLength(string currentString)
        {
            const int MAX_AVAILABLE_STRING_LENGTH = 120;
            if (currentString.Length >= MAX_AVAILABLE_STRING_LENGTH)
            {
                string errorMessage = "Превышена рекомендуемая длина строки";
                CreateObjectLineWithError(errorMessage, currentString);
            }
        }
        
        void CheckEssencesOutsideMethods(string currentString)
        {
            Regex GlobalEssencesRightNaming = new Regex(@"\s*(class|namespace|Type|event|enum|struct)\s[A-Z][a-zA-Z]*\s*");
            //tested
            if (GlobalEssencesRightNaming.IsMatch(currentString))
            {
                return;
            }
            else
            {
                string errorMessage = "Рекомендуется именовать эту сущность в соответствии с нотацией CamelCase";
                CreateObjectLineWithError(errorMessage, currentString);
            }
        }

        void CheckConstNaming(string currentString)
        {
            Regex constantNaming = new Regex(@".*\s*(const)\s(sbyte|byte|short|ushort|int|uint|long|long|char|float|double|decimal|bool)\s([^a-z]*[A-Z]+_*)+.*\s*");
            // tested
            if (constantNaming.IsMatch(currentString))
            {
                return;
            }
            else
            {
                string errorMessage = "Рекомендуется именовать константы в соответствии с нотацией SCREAMING_SNAKE_CASE";
                CreateObjectLineWithError(errorMessage, currentString);
            }
        }

        void CheckForForbiddenWords(string currentString)
        {
            if (currentString.Contains("goto"))
            {
                string errorMessage = "Не рекомендуется использовать оператор goto";
                CreateObjectLineWithError(errorMessage, currentString);
            }
            if (currentString.Contains("flag"))
            {
                string errorMessage = "Не рекомендуется использовать переменные флаги";
                CreateObjectLineWithError(errorMessage, currentString);
            }
        }

        void CheckMethodSignature(string currentString)
        {
            CheckNumberOfArguments(currentString);
            CheckSignatureNaming(currentString);
        }
        void CheckNumberOfArguments(string currentString)
        {
            Regex argumentsInMethod = new Regex(@"(public|private|protected|internal|protected internal)?\s*(virtual|async|extern|new|override|static|unsafe)?\s*(sbyte|byte|short|ushort|int|uint|long|ulong|char|float|double|decimal|bool|void){1}\s*\w*\s*\((?<arguments>(([a-zA-Z\[\]\<\>]*\s*[a-zA-Z]*\s*)[,]?\s*)+)\).*\s*");
            Match argumentsMatch = argumentsInMethod.Match(currentString);

            if (argumentsMatch.Success)
            {
                string argumets = argumentsMatch.Groups["arguments"].Value;
                int numberOfArguments = CountArguments(argumets);

                const int MAX_AVAILABLE_NUMBER_OF_ARGUMENTS = 3;
                if (numberOfArguments > MAX_AVAILABLE_NUMBER_OF_ARGUMENTS)
                {
                    string errorMessage = "Превышено рекомендуемое количество аргументов";
                    CreateObjectLineWithError(errorMessage, currentString);
                }
            }
        }
        private int CountArguments(string allArgumetsInOneString)
        {
            char[] space = { ' ' };
            string[] wordsInStringWithArguments = allArgumetsInOneString.Split(space, StringSplitOptions.RemoveEmptyEntries);
            int numberOfWords = wordsInStringWithArguments.Length;
            int numberOfArguments = numberOfWords / 2;

            return numberOfArguments;
        }

        void CheckSignatureNaming(string currentString)
        {
            Regex rightSignatureNaming = new Regex(@"\s*(public|private|protected|internal|protected internal)?\s*(virtual|async|extern|new|override|static|unsafe)?\s*(sbyte|byte|short|ushort|int|uint|long|ulong|char|float|double|decimal|bool|void){1}\s*[A-Z]{1}\w*\s*\((([a-zA-Z\[\]\<\>]*\s*[a-zA-Z]*\s*)[,]?\s*)+\)\s*");

            if (rightSignatureNaming.IsMatch(currentString))
            {
                return;
            }
            else
            {
                string errorMessage = "Рекомендуется именовать метод в соответствии с нотацией CamelCase";
                CreateObjectLineWithError(errorMessage, currentString);
            }
        }
        void CheckVarNaming(string currentString)
        {
            string[] fieldModificators =
                {"public", "private","protected", "internal", "protected", "internal", "virtual", "async", "extern", "new", "override", "static", "unsafe"};

            foreach (string modificator in fieldModificators)
            {
                if (currentString.Contains(modificator))
                {
                    // Глобальные
                    CheckGlobalVarNaming(currentString);
                    break;
                }
                else
                {
                    // Локальные
                    CheckLocalVarNaming(currentString);
                    break;
                }
            }
        }
        void CheckGlobalVarNaming(string currentString)
        {
            Regex globalVarNaming = new Regex(@"(public|private|protected|internal|protected internal)?\s*(virtual|async|extern|static)?\s*(sbyte|byte|short|ushort|int|uint|long|ulong|char|float|double|decimal|bool){1}\s[A-Z]{1}[a-zA-Z]*\s*.*");

            if (globalVarNaming.IsMatch(currentString))
            {
                return;
            }
            else
            {
                string errorMessage = "Рекомендуется именовать глобальные переменные в соответствии с нотацией CamelCase";
                CreateObjectLineWithError(errorMessage, currentString);
            }
        }
        void CheckLocalVarNaming(string currentString)
        {
            Regex localVarNaming = new Regex(@"\s*(sbyte|byte|short|ushort|int|uint|long|ulong|char|float|double|decimal|bool){1}\s[a-z]{1}[a-zA-Z]*\s*.*");

            if (localVarNaming.IsMatch(currentString))
            {
                return;
            }
            else
            {
                string errorMessage = "Рекомендуется именовать локальные переменные в соответствии с нотацией lowerCamelCase";
                CreateObjectLineWithError(errorMessage, currentString);
            }
        }
        void CheckPercentageOfComments()
        {
            const int MAX_AVAILABLE_COMMENT_PERCENTAGE = 10;

            double currentCommentPercentage = (NumberOfCommentedStrings / NumberOfStrings) * 100.0;
            if (currentCommentPercentage > MAX_AVAILABLE_COMMENT_PERCENTAGE)
            {
                string message = "Превышена рекомендуемая плотность комментариев";
                ErrorMessage errorMessage = new ErrorMessage(message);
                SerializeErrorMessage(errorMessage);
            }
        }



        #region SupportingMethods
        private void CreateObjectLineWithError(string message, string content)
        {
            string errorMessage = message;
            LineWithError lineWithError = new LineWithError(content, errorMessage);
            WrongStringBSerializator.Serialize(WrongStringSerializator, lineWithError);
        }
        private void SerializeErrorMessage(ErrorMessage errorMessage)
        {
            ErrorMessageBSerializator.Serialize(ErrorMessageSerializator, errorMessage);
        }
        private string GetFilePath()
        {
            string filePath = null;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }               
            }
            return filePath;
        }
        bool IsEmpty(string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
        bool IsCommented(string currentString)
        {
            bool isCommented = false;
            Regex lineComment = new Regex(@"//(.*?)\s*");
            Regex blockCommentBeginning = new Regex(@"/\*(.*?)\s*");
            Regex blockCommentEnd = new Regex(@"(.*?)\s*\*\/\s*");
            Regex blockCommentLine = new Regex(@"\s*\*\s*.*\s*");

            if (lineComment.IsMatch(currentString) || blockCommentBeginning.IsMatch(currentString) || blockCommentEnd.IsMatch(currentString) || blockCommentLine.IsMatch(currentString))
            {
                isCommented = true;
            }
            return isCommented;
        }

        bool IsEssenceOutsideMethods(string currentString)
        {
            bool isEssenceOutsideMethods = false;
            string[] EssencesOutsideMethods = { "class", "namespace", "Type", "event", "enum", "struct" };
            foreach (string essence in EssencesOutsideMethods)
            {
                if (currentString.Contains(essence))
                {
                    isEssenceOutsideMethods = true;
                    break;
                }
            }
            return isEssenceOutsideMethods;
        }
        bool IsConst(string currentString)
        {
            bool isConst = false;
            if (currentString.Contains("const"))
            {
                isConst = true;
            }
            return isConst;
        }
        bool ContainsForbiddenWords(string currentString)
        {
            bool containForbiddenWords = false;
            if (currentString.Contains("goto") || currentString.Contains("flag"))
            {
                containForbiddenWords = true;
            }
            return containForbiddenWords;
        }
        bool ContainsMethodSignature(string currentString)
        {
            bool containsMethodSignature = false;
            Regex methodDefaulSignature = new Regex(@"\s*(public|private|protected|internal|protected internal)?\s*(virtual|async|extern|new|override|static|unsafe)?\s*(sbyte|byte|short|ushort|int|uint|long|ulong|char|float|double|decimal|bool|void){1}\s*\w*\s*\((([a-zA-Z\[\]\<\>]*\s*[a-zA-Z]*\s*)[,]?\s*)+\)\s*");
            if (methodDefaulSignature.IsMatch(currentString))
            {
                containsMethodSignature = true;
            }
            return containsMethodSignature;
        }
        bool ContainsVar(string currentString)
        {
            bool containsVar = false;
            string[] variableTypes = { "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "decimal", "bool" };

            foreach (string varType in variableTypes)
            {
                if (currentString.Contains(varType))
                {
                    containsVar = true;
                    break;
                }
            }
            return containsVar;
        }
        private bool NoMistakes()
        {
            bool noMistakes = false;
            int i = 0;
            if (new FileInfo("WrongLines.bin").Length == 0 && new FileInfo("ErrorMessages.bin").Length == 0)
            {
                noMistakes = true;
            }
            return noMistakes;
        }
        #endregion

    }
}
