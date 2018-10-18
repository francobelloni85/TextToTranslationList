using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace TextToTranslationList.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public enum TypeRow { data, comment }

        public ObservableCollection<RowData> WordList { get; set; } = new ObservableCollection<RowData>();

        private string inputText;
        public string InputText {
            get {
                return inputText;
            }
            set {
                inputText = value;
                OnPropertyChanged("InputText");
            }
        }

        private string xmlResoul;
        public string XmlResult {
            get {
                return xmlResoul;
            }
            set {
                xmlResoul = value;
                OnPropertyChanged();
            }
        }

        public string IDUserValue { get; set; }

        public ICommand FirstStepCommand { get; set; }
        public ICommand SecondStepCommand { get; set; }


        public MainWindowViewModel()
        {
            try
            {
                FirstStepCommand = new Command(FromInputToList, ReturnTrue);
                SecondStepCommand = new Command(FromListToXml, ReturnTrue);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

        }

        public class RowData
        {
            public int id;
            public TypeRow TypeRow { get; set; }
            public string Key { get; set; }
            public string Translate { get; set; }
            public Brush ColorTextBox { get; set; }
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private static string RemoveComma(string keyWord)
        {

            // Tolgo la virgola
            if (keyWord[keyWord.Length - 1] == ',')
            {
                keyWord = keyWord.Substring(0, keyWord.Length - 1);
            }

            return keyWord;

        }

        private static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        #region buttons

        private void FromInputToList(object param)
        {
            int ID = 0;
            try
            {
                if (IDUserValue != null)
                {
                    if (IsDigitsOnly(IDUserValue) == true)
                    {
                        ID = Convert.ToInt32(IDUserValue);
                    }
                }

            }
            catch (Exception ex)
            {
                Debugger.Break();
            }


            try
            {


                var result = InputText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                foreach (string item in result)
                {
                    Brush _color = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    TypeRow type = TypeRow.data;
                    string keyWord = item.Trim();

                    // C'è il carattere '='
                    int indexU = keyWord.IndexOf("=");
                    if (indexU != -1)
                    {
                        string numberID = keyWord.Substring(keyWord.IndexOf("=") + 1);

                        keyWord = keyWord.Substring(0, indexU);

                        numberID = numberID.Trim();
                        numberID = RemoveComma(numberID); // Tolgo la virgola
                        numberID = numberID.Trim();
                        if (IsDigitsOnly(numberID) == true)
                        {
                            ID = Convert.ToInt32(numberID);
                            _color = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                        }
                        else
                        {
                            Debugger.Break();
                        }

                    }

                    // Tolgo la virgola
                    keyWord = RemoveComma(keyWord);


                    // Capisco se è un commento
                    if (keyWord[0] == '/')
                    {
                        type = TypeRow.comment;
                        _color = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    }

                    var lastWord = keyWord.Split('_').Last();
                    string traslation = SplitCamelCase(lastWord);
                    traslation = RemoveComma(traslation).Trim();

                    if (type == TypeRow.comment)
                    {
                        traslation = "";
                    }

                    WordList.Add(new RowData()
                    {
                        id = ID,
                        Key = keyWord,
                        Translate = traslation,
                        TypeRow = type,
                        ColorTextBox = _color
                    });


                    // AUmento l'iD solo se non è un commento
                    if (type == TypeRow.data)
                    {
                        ID++;
                    }


                }

                OnPropertyChanged("WordList");
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }


        }

        private bool ReturnTrue(object param)
        {
            return true;
        }

        private void FromListToXml(object para)
        {
            StringBuilder sb = new StringBuilder();
            foreach (RowData item in WordList)
            {

                if (item.TypeRow == TypeRow.data)
                {
                    Translation translation = new Translation();
                    translation.Key = item.id.ToString();

                    List<Entries> entries = new List<Entries>();
                    translation.Entries = entries;

                    foreach (string language in Languages)
                    {

                        var currentEntries = new Entries();

                        string text;
                        // Inglese
                        if (language == Languages[0])
                        {
                            text = item.Translate;
                        }
                        else
                        {
                            text = "[" + item.Translate + "]";
                        }

                        currentEntries.Language = language;
                        currentEntries.Text = text;
                        entries.Add(currentEntries);
                    }

                    sb.Append(SerializeXML(translation));

                }
                else
                {
                    sb.Append("<!--" + item.Key + "-->");
                }

                sb.Append(Environment.NewLine);



            }

            XmlResult = sb.ToString();

        }

        private string SerializeXML(Translation input)
        {


            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");

            //Create the serializer
            XmlSerializer slz = new XmlSerializer(typeof(Translation));

            //Serialize the object with our own namespaces (notice the overload)

            XmlSerializer xsSubmit = new XmlSerializer(typeof(Translation));
            string xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    slz.Serialize(writer, input, ns);

                    //xsSubmit.Serialize(writer, input);
                    xml = sww.ToString(); // Your XML                   
                }
            }

            // Rimuove l'intestazione ed aggiunge un Acapo
            xml = xml.Substring(xml.IndexOf('>') + 1);

            return xml;

        }

        public List<string> Languages = new List<string>() { "eng", "ita", "ger", "fra", "spa", "rus", "zh_HANS", "zh_HANT", "por", "pol", "pt_br", "tur,kor" };

        #endregion

        #region INotifyImplementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public class Command : ICommand
        {
            public event EventHandler CanExecuteChanged;

            private Action<object> executeMethod;
            private Predicate<object> CanExecuteMethod;

            public Command(Action<object> _executeMethod, Predicate<object> _canExecuteChange)
            {
                this.executeMethod = _executeMethod;
                this.CanExecuteMethod = _canExecuteChange;
            }

            public Command(Action<object> ExecuteMethod) : this(ExecuteMethod, null)
            {

            }

            public bool CanExecute(object parameter)
            {
                if (CanExecuteMethod == null)
                    return true;

                return CanExecuteMethod.Invoke(parameter);
            }

            public void Execute(object parameter)
            {
                executeMethod.Invoke(parameter);
            }

            public void RaiseCanExecuteChange()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

        }


        [XmlRoot(ElementName = "Entries")]
        public class Entries
        {
            [XmlElement(ElementName = "Language")]
            public string Language { get; set; }
            [XmlElement(ElementName = "Text")]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Translation")]
        public class Translation
        {
            [XmlElement(ElementName = "Entries")]
            public List<Entries> Entries { get; set; }
            [XmlAttribute(AttributeName = "Key")]
            public string Key { get; set; }
        }


    }

}






