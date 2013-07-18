using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TcxReader.data.xml;

namespace TcxReader
{
    public partial class PointListForm : Form
    {
        public PointListForm()
        {
            InitializeComponent();
        }

        public void Show(List<Trackpoint> l, Form owner)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = l;

            dataGridView1.DataSource = bs;

            this.ShowDialog(owner);
        }
    }
}
