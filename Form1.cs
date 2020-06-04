using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Globalization;

namespace Work
{
    public partial class Form1 : Form
    {
        int optionSearchEngine = 0, ixcurrkey = 0;
        string results = string.Empty, textresults = string.Empty, currkey = string.Empty;
        decimal[] dresgoo, dresbng;
        string[] keywords;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //1 = Google, 2 = Bing
            optionSearchEngine = 1;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string skeys = txtboxSearch.Text.Trim();
            txtResult.Text = string.Empty;
            if (skeys == string.Empty)
                MessageBox.Show("Enter one keyword at least");
            else
            {
                int pix = skeys.IndexOf("\"");
                while (pix > -1)
                {
                    int newpix = skeys.IndexOf("\"", pix + 1);
                    if (newpix == -1)
                    {
                        skeys.Remove(pix, 1);
                        break;
                    }
                    else
                    {
                        string sub = skeys.Substring(pix + 1, newpix - pix);
                        skeys = skeys.Substring(0, pix + 1) + sub.Replace(' ', '$') + skeys.Substring(newpix + 1);
                        pix = newpix;
                    }
                }
                keywords = skeys.Split(' ');
                dresgoo = new decimal[keywords.Length];
                dresbng = new decimal[keywords.Length];
                ixcurrkey = 0;
                currkey = keywords[ixcurrkey].Replace('$', '+');
                webBrowser1.Navigate("https://www.google.com/search?q=" + currkey);
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            HtmlDocument doc = webBrowser1.Document;
            if (doc.Body == null)
                return;
            string shtml = doc.Body.OuterHtml;
            if (optionSearchEngine == 1)
            {
                textresults = Environment.NewLine + "Results " + currkey.Replace('+', ' ') + ":";
                string delim = "result-stats", idxsremove = string.Empty;
                int idx = shtml.IndexOf(delim), ix = 0;
                idx = shtml.IndexOf(delim, idx + delim.Length);
                idx = shtml.IndexOf(delim, idx + delim.Length);
                if (idx > -1)
                {
                    try
                    {
                        results = shtml.Substring(idx + delim.Length, shtml.IndexOf("<", idx + delim.Length) - idx - delim.Length);
                        foreach (char c in results)
                        {
                            ix++;
                            if (!char.IsDigit(c) && c != '.' && c != ',')
                                idxsremove += ix + ",";
                        }
                        string[] sep = { "," };
                        string[] srems = idxsremove.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        for (int x = results.Length - 1; x > -1; x--)
                        {
                            foreach (string s in srems)
                            {
                                int nx = Convert.ToInt32(s);
                                if (nx == x + 1)
                                    results = results.Remove(x, 1);
                                else if (nx > x + 1)
                                    break;
                            }
                        }
                        dresgoo[ixcurrkey] = Convert.ToDecimal(results);
                        textresults += Environment.NewLine + "Google results: " + results;
                    }
                    catch
                    {
                        textresults += Environment.NewLine + "Google results: no results";
                    }
                }
                else
                    textresults += Environment.NewLine + "Google results: no results";
                optionSearchEngine = 2;
                if (currkey != string.Empty)
                    webBrowser1.Navigate("https://www.bing.com/search?q=" + currkey);
            }
            else if (optionSearchEngine == 2)
            {
                string delim = "sb_count";
                try
                {
                    int idx = shtml.IndexOf(delim);
                    idx = shtml.IndexOf(">", idx + delim.Length);
                    results = shtml.Substring(idx + 1, shtml.IndexOf(' ', idx + 1) - idx - 1);
                    dresbng[ixcurrkey] = Convert.ToDecimal(results.Replace(".", ","), CultureInfo.InvariantCulture);
                    txtResult.Text += textresults + Environment.NewLine + "Bing results: " + results;
                }
                catch (Exception ex)
                {
                    string smsg = ex.Message;
                    txtResult.Text += textresults + Environment.NewLine + "Bing results: no results";
                }
                optionSearchEngine = 1;
                ixcurrkey++;
                if (ixcurrkey < keywords.Length)
                {
                    currkey = keywords[ixcurrkey].Replace('$', '+');
                    webBrowser1.Navigate("https://www.google.com/search?q=" + currkey);
                }
                else 
                {
                    int imaxgoo = 0, imaxbng = 0, imaxtot = 0;
                    decimal piv = 0, piv2 = 0, piv3 = 0;
                    string sres = string.Empty;
                    for (int xx = 0; xx < dresgoo.Length; xx++)
                    {
                        if (dresgoo[xx] > piv)
                        {
                            piv = dresgoo[xx];
                            imaxgoo = xx;
                        }
                        if (dresbng[xx] > piv2)
                        {
                            piv2 = dresbng[xx];
                            imaxbng = xx;
                        }
                        if (dresgoo[xx] + dresbng[xx] > piv3)
                        {
                            piv3 = dresgoo[xx] + dresbng[xx];
                            imaxtot = xx;
                        }
                    }
                    txtResult.Text += Environment.NewLine + "Google winner = " + keywords[imaxgoo].Replace('$', ' ');
                    txtResult.Text += Environment.NewLine + "Bing winner = " + keywords[imaxbng].Replace('$', ' ');
                    txtResult.Text += Environment.NewLine + "Total winner = " + keywords[imaxtot].Replace('$', ' ');
                }
            }
        }
    }
}

