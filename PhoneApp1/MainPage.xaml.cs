using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace PhoneApp1
{

    public partial class MainPage : PhoneApplicationPage
    {
        wsServico.cadastroTo cadastroTo = null;

        public MainPage()
        {
            InitializeComponent();
        }

        private void btnSoap_Click(object sender, RoutedEventArgs e)
        {
            getSoap();
        }

        private void btnRest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebClient webClient = new WebClient();
                string parametro1 = "A";
                string parametro2 = "B";
                string urlRest = "http://192.168.1.30:8080/test/testejavarest/" + parametro1 + "/" + parametro2;
                Uri uri = new Uri(urlRest);
                webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(OpenReadCompletedTest);
                webClient.OpenReadAsync(uri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void OpenReadCompletedTest(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                StreamReader reader = new StreamReader(e.Result);
                StringBuilder output = new StringBuilder();                
                string XML = reader.ReadToEnd().ToString();
                XmlReader xmlReader = XmlReader.Create(new StringReader(XML));
                string cadastroId = null;
                string quantidade = null;

                while (xmlReader.Read())
                {
                    if (xmlReader.IsStartElement())
		            {
                    
                        if (xmlReader.Name=="cadastroId")
                        {   
                            if (xmlReader.Read())
			                {
				                cadastroId = xmlReader.Value.Trim();
			                }
                            
                        }
                        
                        if (xmlReader.Name == "quantidade")
                        {
                            if (xmlReader.Read())
                            {
                                quantidade = xmlReader.Value.Trim();                    
                            }
                            
                        }                               
                   }
                }

                output.Append("Id:" + cadastroId);
                output.Append("\r\n");                    
                output.Append("Quantidade:" + quantidade);
                output.Append("\r\n");

                txtRest.Text = output.ToString();
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
		
		public void getSoap()
        {

            HttpWebRequest spAuthReq = HttpWebRequest.Create("http://192.168.1.30:8080/test/TesteJavaEndPointService?wsdl") as HttpWebRequest;
            //spAuthReq.Headers["SOAPAction"] = "http://192.168.1.30:8080/test/TesteJavaEndPointService/returnServico";
            spAuthReq.ContentType = "text/xml; charset=utf-8";
            spAuthReq.Method = "POST";
            spAuthReq.BeginGetRequestStream(new AsyncCallback(selectedGetSoap), spAuthReq);
        }

        private void selectedGetSoap(IAsyncResult asyncResult)
        {
            string parametro1 = "C";
            string parametro2 = "D";

            string AuthEnvelope =
                        @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""http://localhost:8080/test"">
                            <soapenv:Header/>
                            <soapenv:Body>
                                <web:returnServico>
                                    <parametro1>" + parametro1 + @"</parametro1>
                                    <parametro2>" + parametro2 + @"</parametro2>
                                </web:returnServico>
                            </soapenv:Body>
                            </soapenv:Envelope>";

            UTF8Encoding encoding = new UTF8Encoding();
            HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
            System.Diagnostics.Debug.WriteLine("REquest is :" + request.Headers);
            Stream _body = request.EndGetRequestStream(asyncResult);
            string envelope = string.Format(AuthEnvelope);
            System.Diagnostics.Debug.WriteLine("Envelope is :" + envelope);
            byte[] formBytes = encoding.GetBytes(envelope);
            _body.Write(formBytes, 0, formBytes.Length);
            _body.Close();
            request.BeginGetResponse(new AsyncCallback(getAllGetSoapCallback), request);
        }

        private void getAllGetSoapCallback(IAsyncResult asyncResult)
        {

            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    System.Diagnostics.Debug.WriteLine("Async Result is :" + asyncResult);
                    HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult);
                    if (request != null && response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {

                            StreamReader reader = new StreamReader(response.GetResponseStream());
                            string Notificationdata = Util.RemoveAllNamespaces(reader.ReadToEnd());
                            XmlReader xmlReader = XmlReader.Create(new StringReader(Notificationdata));
                            
                            cadastroTo = new wsServico.cadastroTo();
                            
                            while (xmlReader.Read())
                            {
                                if (xmlReader.IsStartElement())
                                {
                                    if (xmlReader.Name == "cadastroId")
                                    {
                                        if (xmlReader.Read())
                                        {
                                            cadastroTo.cadastroId = Convert.ToInt64(xmlReader.Value.Trim());
                                        }
                                    }

                                    if (xmlReader.Name == "quantidade")
                                    {
                                        if (xmlReader.Read())
                                        {
                                            cadastroTo.quantidade = Convert.ToInt32(xmlReader.Value.Trim());
                                        }
                                    }
                                }
                            }
                            reader.Close();
                        }
                        StringBuilder output = new StringBuilder();
                        output.Append("Id:" + cadastroTo.cadastroId);
                        output.Append("\r\n");
                        output.Append("Quantidade:" + cadastroTo.quantidade);
                        output.Append("\r\n");

                        Dispatcher.BeginInvoke(() =>
                        {
                            txtSoap.Text = output.ToString(); 
                        }); 
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("A rede nao esta disponivel"));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show(ex.Message));
            }
        }

        private void btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            txtSoap.Text = "";
            txtRest.Text = "";
        }

    }
    
}