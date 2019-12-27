using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Windows.Controls.Primitives;

namespace busstops
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowMainPanel();
        }

        private void ShowMainPanel(object sender, RoutedEventArgs e)
        {
            HidePanels("MainPanel");
            FillRoutes();
        }

        private void ShowMainPanel()
        {
            HidePanels("MainPanel");
            FillRoutes();
        }

        private void ShowDBPanel(object sender, RoutedEventArgs e)
        {
            try
            {
                HidePanels("DBPanel");
                MenuItem menubutton = (MenuItem)sender;
                SqlConnection con = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True");
                con.Open();
                SqlCommand cmd;
                if (menubutton.Name == "Arrive_Time")
                {
                    cmd = new SqlCommand("select Arrive_Time.id,Route.number,Trip.id as Trip, Stop.name, Arrive_Time.time  from Arrive_Time join Stop on Stop.id = Arrive_Time.stop join Trip on Trip.id = Arrive_Time.trip join Route on Route.id = Trip.route", con);
                }
                else
                {
                    cmd = new SqlCommand("select * from " + menubutton.Name, con);
                }
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                BusGrid.ItemsSource = dt.DefaultView;
                cmd.Dispose();
                con.Close();
            }
            catch
            {
                MessageBox.Show("Неверынй 1255 формат данных");
            }
        }

        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.Controls.DataGridRow;
                if (null != row) yield return row;
            }

        }
        private void UpdateDB(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, TimeSpan> new_stops = new Dictionary<string, TimeSpan> { };
                var temp_height = ArriveInAddDBGrid.Height;
                ArriveInAddDBGrid.Height = 10000;
                for (int i = 0; i < ArriveInAddDBGrid.Items.Count - 1; i++)
                {
                    //loop throught cell
                    TextBlock st_tb = GetCell(i, 0).Content as TextBlock;
                    TextBlock tm_tb = GetCell(i, 1).Content as TextBlock;
                    new_stops.Add(st_tb.Text, TimeSpan.Parse(tm_tb.Text));
                }
                ArriveInAddDBGrid.Height = temp_height;
                using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
                {
                    myConnection.Open();
                    foreach (var name in new_stops)
                    {
                        try
                        {
                            SqlCommand command = new SqlCommand("INSERT INTO [Stop] (name) VALUES (@name)", myConnection);
                            command.Parameters.AddWithValue("@name", name.Key);
                            command.ExecuteNonQuery();
                        }
                        catch
                        {
                           
                        }
                    }
                    Dictionary<string, int> stop_ids = new Dictionary<string, int>();
                    string oString = "SELECT name,id FROM Stop where name in ('" + string.Join("','", new_stops.Keys) + "')"; ;
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            stop_ids.Add(oReader["name"].ToString(), Convert.ToInt32(oReader["id"]));
                        }
                    }
                    foreach (var stop in stop_ids)
                    {
                        try
                        {
                            SqlCommand command = new SqlCommand("INSERT INTO [Arrive_time] (stop,trip,time) VALUES (@stop,@trip,@time)", myConnection);
                            command.Parameters.AddWithValue("@stop", stop.Value);
                            command.Parameters.AddWithValue("@trip", Temp.Content);
                            command.Parameters.AddWithValue("@time", new_stops[stop.Key]);
                            command.ExecuteNonQuery();
                        }
                        catch
                        {

                        }
                    }
                    Dictionary<int, TimeSpan> new_time = new Dictionary<int, TimeSpan> { };
                    Dictionary<int, TimeSpan> last_time = new Dictionary<int, TimeSpan> { };
                    foreach (var i in new_stops)
                    {
                        last_time.Add(stop_ids[i.Key], i.Value);
                    }

                    oString = "SELECT stop, time FROM Arrive_time where trip = " + Temp.Content;
                    oCmd = new SqlCommand(oString, myConnection);
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            new_time.Add(Convert.ToInt32(oReader["stop"]), (TimeSpan)oReader["time"]);
                        }
                    }
                    foreach (var i in new_time)
                    {
                        try
                        {
                            if (i.Value != last_time[i.Key])
                            {
                                SqlCommand command = new SqlCommand("UPDATE Arrive_Time SET time = @time Where stop = @id and trip = " + Temp.Content, myConnection);
                                command.Parameters.AddWithValue("@id", i.Key);
                                command.Parameters.AddWithValue("@time", last_time[i.Key].ToString());
                                command.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            try
                            {
                                SqlCommand command = new SqlCommand("Delete from Arrive_Time Where stop = @id and trip = " + Temp.Content, myConnection);
                                command.Parameters.AddWithValue("@id", i.Key);
                                command.ExecuteNonQuery();
                            }
                            catch
                            {
                                MessageBox.Show("Неправильный 634634 формат данных");
                            }
                        }
                    }
                    myConnection.Close();
                }
                MessageBox.Show("Updated");
                FillRouteByTrip(sender, e);
            }
            catch
            {
                MessageBox.Show("Неверынй 1 формат данных1");
            }
        }

        public DataGridCell GetCell(int row, int column)
        {
            DataGridRow rowContainer = GetRow(row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    ArriveInAddDBGrid.ScrollIntoView(rowContainer, ArriveInAddDBGrid.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        public DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)ArriveInAddDBGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                ArriveInAddDBGrid.UpdateLayout();
                ArriveInAddDBGrid.ScrollIntoView(ArriveInAddDBGrid.Items[index]);
                row = (DataGridRow)ArriveInAddDBGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        private void AddRoutePanel(object sender, RoutedEventArgs e)
        {
            HidePanels("AddDBRoutePanel");
        }

        private void updateDriver(object sender, RoutedEventArgs e)
        {
            try
            {
                Window2 adddriverwindow = new Window2();
                if (adddriverwindow.ShowDialog() == true)
                {
                    using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
                    {
                        myConnection.Open();
                        try
                        {
                            SqlCommand command = new SqlCommand("UPDATE Bus SET driver = @driver Where number = " + Convert.ToInt32(adddriverwindow.Number.Text), myConnection);
                            command.Parameters.AddWithValue("@driver", adddriverwindow.Name.Text);
                            command.ExecuteNonQuery();
                        }
                        catch
                        {
                            try
                            {
                                SqlCommand command = new SqlCommand("INSERT INTO [Bus] (driver,number) VALUES (@driver,@number)", myConnection);
                                command.Parameters.AddWithValue("@driver", adddriverwindow.Name.Text);
                                command.Parameters.AddWithValue("@number", Convert.ToInt32(adddriverwindow.Number.Text));
                                command.ExecuteNonQuery();
                            }
                            catch
                            {
                            }
                        }
                        myConnection.Close();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Неверынй2 формат данных");
            }
        }
        private void AddTrip(object sender, RoutedEventArgs e)
        {
            try
            {
                Window1 passwordWindow = new Window1();
                if (passwordWindow.ShowDialog() == true)
                {
                    Window3 days = new Window3();
                    if (days.ShowDialog() == true)
                    {
                        Button button = (Button)sender;
                        using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
                        {
                            myConnection.Open();
                            string oString = "Select * from Route Where number =  " + RouteNumberInAddDBPanel.Text.ToString();
                            SqlCommand oCmd = new SqlCommand(oString, myConnection);
                            int y = 0;
                            string value = "0";
                            using (SqlDataReader oReader = oCmd.ExecuteReader())
                            {
                                while (oReader.Read())
                                {
                                    y += 1;

                                }
                            }
                            if (y == 0)
                            {
                                oCmd = new SqlCommand("INSERT INTO [Route] (number) VALUES (@number)", myConnection);
                                oCmd.Parameters.AddWithValue("@number", RouteNumberInAddDBPanel.Text.ToString());
                                oCmd.ExecuteNonQuery();
                            }
                            string bus_id = "0";
                            oString = "Select * from Bus Where number =  " + passwordWindow.buscombobox.SelectedValue.ToString();
                            oCmd = new SqlCommand(oString, myConnection);
                            using (SqlDataReader oReader = oCmd.ExecuteReader())
                            {
                                while (oReader.Read())
                                {
                                    bus_id = oReader["id"].ToString();

                                }
                            }
                            oString = "Select * from Route Where number =  " + RouteNumberInAddDBPanel.Text.ToString();
                            oCmd = new SqlCommand(oString, myConnection);
                            using (SqlDataReader oReader = oCmd.ExecuteReader())
                            {
                                while (oReader.Read())
                                {
                                    value = oReader["id"].ToString();

                                }
                            }
                            int days_type;
                            int.TryParse(string.Join("", days.daystypecombobox.SelectedValue.ToString().Where(c => char.IsDigit(c))), out days_type);
                            SqlCommand command = new SqlCommand("INSERT INTO [Trip] (route,bus,days_type) VALUES (@route,@bus,@days_type)", myConnection);
                            command.Parameters.AddWithValue("@route", value);
                            command.Parameters.AddWithValue("@bus", bus_id);
                            command.Parameters.AddWithValue("@days_type", days_type);
                            command.ExecuteNonQuery();
                            myConnection.Close();
                        }
                    }
                    FillTrip(sender, e);
                }
            }
            catch
            {
                MessageBox.Show("Неверынй формат данных");
            }
        }


        private void FillTrip1(object sender, RoutedEventArgs e)
        {
            using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
            {
                myConnection.Open();
                string oString = "Delete from Trip WHERE NOT EXISTS (SELECT 1 FROM Arrive_Time a WHERE Trip.id = a.trip);";
                SqlCommand oCmd = new SqlCommand(oString, myConnection);
                oCmd.ExecuteNonQuery();
                oString = "Delete from Route WHERE NOT EXISTS (SELECT 1 FROM Trip a WHERE Route.id = a.route);";
                oCmd = new SqlCommand(oString, myConnection);
                oCmd.ExecuteNonQuery();
                myConnection.Close();
            }
            FillTrip(sender, e);
        }
        private void FillTrip(object sender, RoutedEventArgs e)
        {
            try
            {
                Update.Visibility = Visibility.Hidden;
                using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
                {
                    myConnection.Open();
                    AddDBRoutePanelTrip.Children.Clear();
                    ArriveInAddDBGrid.ItemsSource = null;
                    string oString = "Select Trip.id, Route.id as route  from Route join Trip on Route.id = Trip.route Where Route.number = " + RouteNumberInAddDBPanel.Text;
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);
                    int y = 0;
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        Button _button;
                        while (oReader.Read())
                        {
                            _button = new Button();
                            _button.Height = 75;
                            _button.Width = 75;
                            _button.Margin = new Thickness(5);
                            _button.Content = oReader["id"].ToString();
                            _button.Click += FillRouteByTrip;
                            AddDBRoutePanelTrip.Children.Add(_button);
                            y += 1;
                        }
                        _button = new Button();
                        _button.Height = 75;
                        _button.Width = 75;
                        _button.FontSize = 50;
                        _button.Margin = new Thickness(5);
                        _button.Content = "+";
                        _button.Click += AddTrip;
                        AddDBRoutePanelTrip.Children.Add(_button);
                    }
                    if (y == 0)
                    {
                        Button _button = new Button();
                        _button.Content = "+";
                        AddTrip(_button, e);
                    }
                    myConnection.Close();
                }
            }
            catch
            {
                MessageBox.Show("Неверынй  5 формат данных");
            }
        }

        private void FillRouteByTrip(object sender, RoutedEventArgs e)
        {
            try
            {
                Update.Visibility = Visibility.Visible;
                try
                {
                    Button button = (Button)sender;
                    Temp.Content = Convert.ToInt32(button.Content);
                }
                catch
                {
                }

                SqlConnection con = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True");
                con.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select Stop.name, Arrive_Time.time  from Arrive_Time join Stop on Stop.id = Arrive_Time.stop join Trip on Trip.id = Arrive_Time.trip join Route on Route.id = Trip.route Where Trip.id = " + Temp.Content + " order by Arrive_Time.time", con);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                ArriveInAddDBGrid.ItemsSource = dt.DefaultView;
                cmd.Dispose();
                con.Close();
            }
            catch
            {
                MessageBox.Show("Неверынй 2 формат данных");
            }
        }
        private void HidePanels(string name)
        {
            foreach (var i in new Grid[] { MainPanel, DBPanel, RoutePanel, AddDBRoutePanel })
            {
                if (i.Name != name)
                {
                    i.Visibility = Visibility.Hidden;
                }
                else
                {
                    i.Visibility = Visibility.Visible;
                }
            }
        }

        private void ShowRoutePanel(object sender, RoutedEventArgs e)
        {
            try
            {
                HidePanels("RoutePanel");
                Button _button = (Button)sender;
                int RouteId;
                int.TryParse(string.Join("", _button.Name.Where(c => char.IsDigit(c))), out RouteId);
                RouteNumber.Content = _button.Content;
                Dictionary<string, string[]> arrive_time = new Dictionary<string, string[]>();
                using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
                {
                    Stops.Children.Clear();
                    string oString;
                    TimeSpan now = DateTime.Now - DateTime.Now.Date;
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        oString = "SELECT Stop.id AS id,name,time FROM Arrive_Time join Stop on Arrive_Time.stop = Stop.id join Trip on Arrive_Time.trip = Trip.id join Route on Trip.route = Route.id join Days_Type on Trip.days_type = Days_Type.id where Route.number = " + _button.Content + "and Days_Type.id = 2 order by ABS(DATEDIFF(Second, time, " + now.Hours + '.' + now.Minutes + "))";
                    }
                    else
                    {
                        oString = "SELECT Stop.id AS id,name,time FROM Arrive_Time join Stop on Arrive_Time.stop = Stop.id join Trip on Arrive_Time.trip = Trip.id join Route on Trip.route = Route.id join Days_Type on Trip.days_type = Days_Type.id where Route.number = " + _button.Content + "and Days_Type.id = 1 order by ABS(DATEDIFF(Second, time, " + now.Hours + '.' + now.Minutes + "))";
                    }
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);
                    myConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            try
                            {
                                if ((TimeSpan)oReader["time"] > now)
                                {
                                    arrive_time.Add(oReader["name"].ToString(), new string[2] { oReader["id"].ToString(), Convert.ToString(Convert.ToInt32(((TimeSpan)oReader["time"] - now).TotalMinutes)) });
                                }
                                else
                                {
                                    arrive_time.Add(oReader["name"].ToString(), new string[2] { oReader["id"].ToString(), Convert.ToString(Convert.ToInt32((new TimeSpan(24, 0, 0) - now + (TimeSpan)oReader["time"]).TotalMinutes)) });
                                }
                            }
                            catch
                            {

                            }
                        }

                        myConnection.Close();
                    }
                }
                arrive_time = arrive_time.OrderBy(key => System.Convert.ToInt32(key.Value[0])).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
                foreach (var pair in arrive_time)
                {
                    TextBox stop = new TextBox();
                    stop.FontSize = 25;
                    stop.FontFamily = new FontFamily("Tele-Marin");
                    stop.Text += "• " + pair.Value[1] + "м " + pair.Key;
                    stop.IsEnabled = false;
                    Stops.Children.Add(stop);
                }
            }
            catch
            {
                MessageBox.Show("Неверынй 21 формат данных");
            }

        }
        public void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void FillRoutes()
        {
            try
            {
                wrapPanel.Children.Clear();
                List<Button> buttons = new List<Button> { };
                using (SqlConnection myConnection = new SqlConnection(@"Data Source=WIN-IEUAMMVRABR\SQLEXPRESS;Initial Catalog=MaxKravtsevich;Integrated Security=True"))
                {
                    string oString = "Select * from Route";
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);
                    myConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            Button _button = new Button();
                            _button.Name = "id" + oReader["id"].ToString();
                            _button.Height = 75;
                            _button.Width = 75;
                            _button.Margin = new Thickness(5);
                            _button.Content = oReader["number"].ToString();
                            _button.Click += ShowRoutePanel;
                            buttons.Add(_button);
                        }

                        myConnection.Close();
                    }
                }
                for (int i = 0; i < buttons.Count; i++)
                {
                    wrapPanel.Children.Add(buttons[i]);
                }
            }
            catch
            {
                MessageBox.Show("Неверынй  15 формат данных");
            }
        }
    }
}
