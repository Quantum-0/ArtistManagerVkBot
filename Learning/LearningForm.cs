using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Learning
{
    public partial class LearningForm : Form
    {
        Learning Learning;
        public LearningForm(Learning learning)
        {
            InitializeComponent();
            
            Learning = learning;
            listBox1.Items.Clear();
            listBoxAnswers.Items.Clear();
            listBoxNotAnswered.Items.Clear();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var answers = Learning.CustomAnswers[listBox1.SelectedItem.ToString()];
            listBoxAnswers.Items.Clear();
            listBoxAnswers.Items.AddRange(answers.Cast<object>().ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var item = listBox1.SelectedItem.ToString();
            Learning.CustomAnswers.Remove(item);
            listBox1.Items.Remove(item);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var index = listBoxAnswers.SelectedIndex;
            Learning.CustomAnswers[listBox1.SelectedItem.ToString()].RemoveAt(index);
            listBoxAnswers.Items.RemoveAt(index);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var item = listBoxNotAnswered.SelectedItem.ToString();
            Learning.NotAnswered.Remove(item);
            listBoxNotAnswered.Items.Remove(item);
        }
    }
}
