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
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
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
            buscombobox.Items.Clear();
            SqlConnection sqlConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True");
            {
                SqlCommand sqlCmd = new SqlCommand("SELECT * FROM BUS", sqlConnection);
                sqlConnection.Open();
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    buscombobox.Items.Add(sqlReader["number"].ToString());
                }

                sqlReader.Close();
            }
        }

        private void AddDriver(object sender, RoutedEventArgs e)
        {
            Window2 adddriverwindow = new Window2();
            if (adddriverwindow.ShowDialog() == true)
            {
                using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
                {
                    myConnection.Open();
                    try
                    {
                        SqlCommand command = new SqlCommand("UPDATE Bus SET driver = @driver Where stop = @id and number = " + Convert.ToInt32(adddriverwindow.Number.Text), myConnection);
                        command.Parameters.AddWithValue("@driver", adddriverwindow.Name.Text);
                        command.ExecuteNonQuery();
                    }
                    catch
                    {
                        SqlCommand command = new SqlCommand("INSERT INTO [Bus] (driver,number) VALUES (@driver,@number)", myConnection);
                        command.Parameters.AddWithValue("@driver", adddriverwindow.Name.Text);
                        command.Parameters.AddWithValue("@number", Convert.ToInt32(adddriverwindow.Number.Text));
                        command.ExecuteNonQuery();
                    }
                    myConnection.Close();
                }
                FillCombo();
            }
        }
    }
}
