using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace CursovayaWF
{
    public partial class Output : Form
    {
        public Output()
        {
            InitializeComponent();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private FileStream WrongStringDeserializator = new FileStream("WrongLines.bin", FileMode.Open);
        private BinaryFormatter WrongStringBDeserializator = new BinaryFormatter();

        private FileStream ErrorMessageDeserializator = new FileStream("ErrorMessages.bin", FileMode.Open);
        private BinaryFormatter ErrorMessageBDeserializator = new BinaryFormatter();
        public void ShowResult()
        {
            
            TryFillGridViewForLines();
            TryFillGridViewForMessages();
        }
        
        private void TryFillGridViewForLines()
        {
            try
            {
                FillGridViewForLines();
            }
            catch (SerializationException)
            {
                WrongStringDeserializator.Close();
            }
        }
        private void FillGridViewForLines()
        {
            int i = 1;
            while (WrongStringDeserializator.CanRead)
            {
                LineWithError lineWithError = (LineWithError)WrongStringBDeserializator.Deserialize(WrongStringDeserializator);

                string content = lineWithError.Content;
                string errorMessage = lineWithError.ErrorMessage;

                dataGridView1.Rows.Add(i, content, errorMessage);
                i++;
            }
        }

        private void TryFillGridViewForMessages()
        {
            try
            {
                FillGridViewForMessages();
            }
            catch (SerializationException)
            {
                ErrorMessageDeserializator.Close();
            }
        }
        private void FillGridViewForMessages()
        {
            int i = 1;
            while (ErrorMessageDeserializator.CanRead)
            {
                ErrorMessage errorMessage = (ErrorMessage)ErrorMessageBDeserializator.Deserialize(ErrorMessageDeserializator);
                string message = errorMessage.Message;

                dataGridView2.Rows.Add(i, message);
                i++;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
