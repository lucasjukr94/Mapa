using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms.VisualStyles;
using GoogleMaps.LocationServices;

namespace Mapa
{
    public partial class Form1 : Form
    {
        private int sucess = 0;
        private int erro = 0;
        private int totalarmazenado = 0;
        private string arquivo = @"UltimaBusca.dat";
        private Timer timer1 = new Timer();
        private int buscas, tempo;

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = sucess.ToString();
            label4.Text = erro.ToString();
            label12.Text = ((((int.TryParse(textBox1.Text, out buscas)?buscas:0) * (int.TryParse(textBox2.Text, out tempo) ? tempo : 0))/1000)/60).ToString() + " min";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int teste1 = int.Parse(textBox2.Text);
                try
                {
                    int teste = int.Parse(textBox1.Text);
                    string connectionstring = @"Data Source=LOTESTX8\SQL2012;Initial Catalog=darwin;Integrated Security=True";
                    SqlConnection sqlConn = new SqlConnection(connectionstring);
                    string queryBuscaEndereco = @"select top " + textBox1.Text
                        + " c.CEP,c.CEPComTraco,c.Estado,c.Endereco,c.Cidade,c.Bairro from v_Cep as c left join tbl_Cep_Lat_Long as LL on LL.CEP = c.CEP where LL.CEP is null and c.Estado = 'SP'";
                    SqlCommand sqlComm = new SqlCommand(queryBuscaEndereco, sqlConn);

                    EnderecoVm enderecoVm = new EnderecoVm();
                    List<EnderecoVm> listaendereco = new List<EnderecoVm>();

                    try
                    {
                        sqlConn.Open();

                        SqlDataReader reader = sqlComm.ExecuteReader();
                        while (reader.Read())
                        {
                            enderecoVm.CEP = reader["CEP"].ToString();
                            enderecoVm.CEPComTraco = reader["CEPComTraco"].ToString();
                            enderecoVm.Estado = reader["Estado"].ToString();
                            enderecoVm.Endereco = reader["Endereco"].ToString();
                            enderecoVm.Cidade = reader["Cidade"].ToString();
                            enderecoVm.Bairro = reader["Bairro"].ToString();

                            listaendereco.Add(enderecoVm);
                            //reader.NextResult();
                        }
                        reader.Close();
                    }
                    catch (Exception)
                    {
                        erro++;
                    }
                    finally
                    {
                        sqlConn.Close();
                    }

                    string address = @"";
                    string queryInsereLatLong = @"insert into tbl_Cep_Lat_Long(
                                                CEP,
                                                CEPComTraco,
                                                Estado,
                                                Endereco,
                                                Cidade,
                                                Bairro,
                                                Latitude,
                                                Longitude
                                        ) values(
                                                @CEP,
                                                @CEPComTraco,
                                                @Estado,
                                                @Endereco,
                                                @Cidade,
                                                @Bairro,
                                                @Latitude,
                                                @Longitude)";
                    var locationService = new GoogleLocationService();
                    var point = new MapPoint();

                    foreach (var p in listaendereco)
                    {
                        address = p.Endereco + " ," + p.Bairro + " ," + p.Cidade + " ," + p.CEP + " ," + p.Estado;
                        try
                        {
                            System.Threading.Thread.Sleep(int.Parse(textBox2.Text));
                            point = locationService.GetLatLongFromAddress(address);
                            p.lati = point.Latitude;
                            p.longi = point.Longitude;

                            if (p.lati != 0 || p.longi != 0)
                            {
                                try
                                {
                                    sqlConn.Open();
                                    sqlComm.CommandText = queryInsereLatLong;
                                    sqlComm.Parameters.Clear();
                                    sqlComm.Parameters.AddWithValue("CEP", p.CEP);
                                    sqlComm.Parameters.AddWithValue("CEPComTraco", p.CEPComTraco);
                                    sqlComm.Parameters.AddWithValue("Estado", p.Estado);
                                    sqlComm.Parameters.AddWithValue("Endereco", p.Endereco);
                                    sqlComm.Parameters.AddWithValue("Cidade", p.Cidade);
                                    sqlComm.Parameters.AddWithValue("Bairro", p.Bairro);
                                    sqlComm.Parameters.AddWithValue("Latitude", p.lati.ToString());
                                    sqlComm.Parameters.AddWithValue("Longitude", p.longi.ToString());
                                    sqlComm.ExecuteNonQuery();
                                    sucess++;
                                }
                                catch (Exception)
                                {
                                    erro++;
                                }
                                finally
                                {
                                    sqlConn.Close();
                                }
                            }
                        }
                        catch (Exception)
                        {
                            erro++;
                        }
                    }

                    try
                    {
                        sqlConn.Open();
                        string queryBuscaTotalArmazenado = @"select count(*) n from tbl_Cep_Lat_Long";
                        sqlComm.CommandText = queryBuscaTotalArmazenado;

                        string n = @"";

                        SqlDataReader reader = sqlComm.ExecuteReader();
                        while (reader.Read())
                        {
                            n = reader["n"].ToString();
                        }
                        reader.Close();

                        label6.Text = n;

                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        sqlConn.Close();
                    }

                    using (StreamWriter sw = new StreamWriter(arquivo))
                    {
                        sw.Write(DateTime.Now);
                    }
                }
                catch (Exception)
                {
                    textBox1.Text = @"####";
                }
            }
            catch (Exception)
            {
                textBox2.Text = @"####";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(arquivo))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(arquivo))
                    {
                        string linha;
                        while ((linha = sr.ReadLine()) != null)
                        {
                            label8.Text += linha;
                        }

                        if (label8.Text == "")
                        {
                            label8.Text = @"Sem Busca";
                        }
                    }
                }
                catch (Exception)
                {
                    label8.Text = @"Sem Busca";
                }
            }

            timer1.Interval = 10;
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }
    }
}
