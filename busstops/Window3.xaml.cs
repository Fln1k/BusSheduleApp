using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace busstops
{
    /// <summary>
    /// Логика взаимодействия для Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        public Window3()
        {
            InitializeComponent();
            FillCombo();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void FillCombo()
        {
            daystypecombobox.Items.Clear();
            SqlConnection sqlConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True");
            {
                SqlCommand sqlCmd = new SqlCommand("SELECT * FROM Days_Type", sqlConnection);
                sqlConnection.Open();
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();

                while (sqlReader.Read())
                {
                    daystypecombobox.Items.Add(sqlReader["id"].ToString() + ". "+sqlReader["type"].ToString());
                }

                sqlReader.Close();
            }
        }
    }
}
