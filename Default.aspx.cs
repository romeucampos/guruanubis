using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class guru_Default : System.Web.UI.Page
{

    public class ReturnDataArray
    {
        public double[] arrayPriceClose = null;
        public double[] arrayPriceHigh = null;
        public double[] arrayPriceLow = null;
        public double[] arrayPriceOpen = null;
        public double[] arrayVolume = null;
        public double[] arrayDate = null;
        public double[] arrayQuoteVolume = null;
    }

    public static ReturnDataArray getDataArray(string coin, string timeGraph,string limit = "1000")
    {
        System.Threading.Thread.Sleep(6000);
        ReturnDataArray returnDataArray = new ReturnDataArray();
        String jsonAsString = Http.get("https://api.binance.com/api/v1/klines?symbol=" + coin + "&interval=" + timeGraph + "&limit=" + limit);
        Newtonsoft.Json.Linq.JContainer json = (Newtonsoft.Json.Linq.JContainer)JsonConvert.DeserializeObject(jsonAsString);

        returnDataArray.arrayPriceClose = new double[json.Count];
        returnDataArray.arrayPriceHigh = new double[json.Count];
        returnDataArray.arrayPriceLow = new double[json.Count];
        returnDataArray.arrayPriceOpen = new double[json.Count];
        returnDataArray.arrayVolume = new double[json.Count];
        returnDataArray.arrayDate = new double[json.Count];
        returnDataArray.arrayQuoteVolume = new double[json.Count];
        int i = 0;
        foreach (JContainer element in json.Children())
        {
            returnDataArray.arrayPriceClose[i] = double.Parse(element[4].ToString());
            returnDataArray.arrayPriceHigh[i] = double.Parse(element[2].ToString());
            returnDataArray.arrayPriceLow[i] = double.Parse(element[3].ToString());
            returnDataArray.arrayPriceOpen[i] = double.Parse(element[1].ToString());
            returnDataArray.arrayVolume[i] = double.Parse(element[5].ToString());
            returnDataArray.arrayQuoteVolume[i] = double.Parse(element[7].ToString());
            returnDataArray.arrayDate[i] = double.Parse(element[6].ToString());
            i++;
        }

        return returnDataArray;
    }


    public class Perceptron
    {
        private double[] bias;
        private double[,] weight;

        private int totalEntry;
        private int totalExit;

        public Perceptron(int entry, int exit)
        {
            this.totalEntry = entry;
            this.totalExit = exit;
            bias = new double[this.totalExit];
            weight = new double[this.totalExit, this.totalEntry];
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < this.totalEntry; i++)
                for (int j = 0; j < totalExit; j++)
                    weight[j, i] = rnd.NextDouble();
        }

        public void Train(double[] entry, double[] target)
        {
            double[] actual = Compute(entry);
            for (int i = 0; i < this.totalExit; i++)
            {
                for (int j = 0; j < this.totalEntry; j++)
                    weight[i, j] += (target[i] - actual[i]) * entry[j];
                bias[i] += (target[i] - actual[i]);
            }
        }

        public double[] Compute(params double[] entry)
        {
            double[] exit = new double[this.totalExit];
            for (int i = 0; i < this.totalExit; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < entry.Length; j++)
                    sum += weight[i, j] * entry[j];
                sum += bias[i];
                exit[i] = Segmoid(sum);
            }
            return exit;
        }

        private double Segmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }
    }
     void generateIA(string pair = "BTCUSDT", string interval = "1d", int totalEntry = 5, int totalExit = 1, int totalEpochs = 99999, int round = 5)
    {
        try
        {
            string separator = ".";
            ReturnDataArray returnDataArray = getDataArray(pair, interval, "1000");



            List<double[]> exitList = new List<double[]>();
            List<double[]> entryList = new List<double[]>();

            for (int z = 0; z < returnDataArray.arrayPriceClose.Length; z++)
            {
                try
                {
                    List<double> lineEntry = new List<double>();
                    List<double> lineExit = new List<double>();
                    for (int i = 0; i < totalEntry; i++)
                        lineEntry.Add(double.Parse("0" + separator + Math.Round(returnDataArray.arrayPriceClose[z + i])));
                    lineExit.Add(double.Parse("0" + separator + Math.Round(returnDataArray.arrayPriceClose[z + totalEntry])));

                    exitList.Add(lineExit.ToArray());

                    entryList.Add(lineEntry.ToArray());
                    z += totalEntry - 1;
                }
                catch { }
            }

            double[][] entry = entryList.ToArray();

            double[][] exits = exitList.ToArray();

            Perceptron perceptron = new Perceptron(totalEntry, totalExit);

            for (int i = 0; i < totalEpochs; i++)
                for (int k = 0; k < entry.Length; k++)
                    perceptron.Train(entry[k], exits[k]);


            List<double> lastsPrices = new List<double>();
            returnDataArray = getDataArray(pair, interval, totalEntry.ToString());
            for (int i = 0; i < returnDataArray.arrayPriceClose.Length; i++)
                lastsPrices.Add(double.Parse("0" + separator + Math.Round(returnDataArray.arrayPriceClose[i])));
            double[][] newEntry =
                {
               lastsPrices.ToArray()
            };

            String result = perceptron.Compute(newEntry[0])[0].ToString();
            result = result.Replace("0,", "").Replace("0.", "").Substring(0, round);

            string vies = "BAIXA";
            if (double.Parse(result) > Math.Round(returnDataArray.arrayPriceClose[returnDataArray.arrayPriceClose.Length - 1]))
                vies = "ALTA";

            lblResultado.Text = "O resultado para o próximo intervalo é de: $ " + result + "<br/>";



            lblResultado.Text += "Como o BITCOIN hoje está custando $" + (returnDataArray.arrayPriceClose[returnDataArray.arrayPriceClose.Length - 1]) + " e a previsão para o BITCOIN amanhã é de " + vies;
        }
        catch(Exception ex)
        {
            lblResultado.Text = "ERROR " + ex.Message + ex.StackTrace;
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
      
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        generateIA(txtPair.Text,txtInterval.Text,int.Parse(txtInput.Text), int.Parse(txtOutPut.Text), int.Parse(txtEpoch.Text));
    }
}