using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AxKHOpenAPILib;

namespace dasy_Order
{
    public partial class Form1 : Form
    {
        List<StockGrid> stockGridList;
        List<OutStandingOrder> outStandingOrderList;

        public Form1()
        {
            InitializeComponent();
            loginBtn.Click += buttonClick;
            searchBtn.Click += buttonClick;
            buyBtn.Click+= buttonClick;
            sellBtn.Click += buttonClick;
            changeBtn.Click += buttonClick;
            cancelBtn.Click += buttonClick; 

            codeBox.TextChanged += TextBoxChange;
            axKHOpenAPI1.OnEventConnect += OnEventCnt;
            axKHOpenAPI1.OnReceiveTrData += OnTrData;
            axKHOpenAPI1.OnReceiveChejanData += OnchejanData;
        }

        private void OnchejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            //Console.WriteLine("e.sGubun  " +e.sGubun);
            //Console.WriteLine("e.nItemCnt  " +e.nItemCnt);
            //Console.WriteLine("e.sFIdList: "+e.sFIdList);

            if (e.sGubun.Equals("0")) //접수..체결
            {
                string 종목코드 = axKHOpenAPI1.GetChejanData(9001).Trim();
                string 주문번호 = axKHOpenAPI1.GetChejanData(9203).Trim();
                int 주문수량 = int.Parse(axKHOpenAPI1.GetChejanData(900));
                int 미체결수량 = int.Parse(axKHOpenAPI1.GetChejanData(902));
                string 체결량 = axKHOpenAPI1.GetChejanData(911);

                //Console.WriteLine("종목코드  " + 종목코드);
                //Console.WriteLine("주문번호  " + 주문번호);
                //Console.WriteLine("주문수량: " + 주문수량);
                //Console.WriteLine("미체결수량: " + 미체결수량);
                if (체결량.Length > 0) Console.WriteLine("체결량 = " + 체결량);
                else
                {

                }
            }
            else if (e.sGubun.Equals("1"))//잔고전달
            {

            }
        }

        private void TextBoxChange(object sender, EventArgs e)
        {
            try
            {
                if (sender.Equals(codeBox))
                {
                    if (codeBox.TextLength == 6)
                    {
                        axKHOpenAPI1.SetInputValue("종목코드", codeBox.Text);
                        axKHOpenAPI1.CommRqData("주문종목정보", "opt10001", 0, "5010"); //
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            
        }

        public void OnEventCnt(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0)
            {
                loginIdLab.Text = axKHOpenAPI1.GetLoginInfo("USER_ID");
                string AccNolist = axKHOpenAPI1.GetLoginInfo("ACCNO"); //께좌번호
                string[] AccNoArray = AccNolist.Split(';');
                for (int i = 0; i < AccNoArray.Length; ++i)
                {
                    comboBox1.Items.Add(AccNoArray[i]);
                }
            }
        }
        private void buttonClick(object sender, EventArgs e)
        {
            if (sender.Equals(loginBtn))
            {
                axKHOpenAPI1.CommConnect();
            }

            else if (sender.Equals(searchBtn))
            {
                stockGridList = new List<StockGrid>();

                string accno = comboBox1.Text;
                axKHOpenAPI1.SetInputValue("계좌번호", accno);
                axKHOpenAPI1.SetInputValue("비밀번호", "");
                axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
                axKHOpenAPI1.SetInputValue("조회구분", "2");
                axKHOpenAPI1.CommRqData("계좌잔고평가내역", "opw00018", 0, "5000");
                //잔고평가내역요청 끝
                //미체결내역요청
                axKHOpenAPI1.SetInputValue("계좌번호", accno);
                axKHOpenAPI1.SetInputValue("체결구분", "1");
                axKHOpenAPI1.SetInputValue("매매구분", "0");

                axKHOpenAPI1.CommRqData("실시간미체결요청", "opw10075", 0, "5030");

            }
            else if (sender.Equals(buyBtn))
            {
                if(comboBox1.Text.Length >0 && comboBox1.Text.Length ==6)
                {
                    string 계좌번호 = comboBox1.Text;
                    string 종목코드 = codeBox.Text;
                    int 수량 = (int)numericUpDown1.Value;
                    int 가격 = (int)numericUpDown2.Value;
                    string 거래구분 = comboBox2.Text;
                    거래구분 = 거래구분.Substring(0, 2);
                    int res =axKHOpenAPI1.SendOrder("주식주문", "6000", 계좌번호, 1, 종목코드, 수량, 가격, 거래구분, "");
                    //retirn 값
                    //if(res==0) 미체결 데이타
                    //{}

                }
            }
            else if (sender.Equals(sellBtn))
            {
                if (comboBox1.Text.Length > 0 && comboBox1.Text.Length == 6)
                {
                    string 계좌번호 = comboBox1.Text;
                    string 종목코드 = codeBox.Text;
                    int 수량 = (int)numericUpDown1.Value;
                    int 가격 = (int)numericUpDown2.Value;
                    string 거래구분 = comboBox2.Text;
                    거래구분 = 거래구분.Substring(0, 2);
                    int res = axKHOpenAPI1.SendOrder("주식주문", "6000", 계좌번호, 2, 종목코드, 수량, 가격, 거래구분, "");

                    
                }
            }
            else if (sender.Equals(changeBtn))
            {

            }
            else if (sender.Equals(cancelBtn))
            {

            }

        }

        public void OnTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            //MessageBox.Show("ok!!"+e.sRQName+e.sTrCode);
            if (e.sRQName == "계좌잔고평가내역")
            {
                int buyingPrice2 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총매입금액"));
                int ballanceAsset = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "추정예탁자산"));
                int estimatePrice2 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총평가금액"));
                int estimateProfit = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총평가손익금액"));

                buyingPriceLab.Text = buyingPrice2.ToString();
                ballanceAssetLab.Text = ballanceAsset.ToString();
                estimatePriceLab.Text = estimatePrice2.ToString();
                estimateProfitLab.Text = estimateProfit.ToString();


                ///
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                for (int i = 0; i < count; ++i)
                {
                    double amount = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "보유수량"));//TrimStart('0');
                    string itemName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    double buyingPrice = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "매입가"));
                    double currentPrice = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    double estimateProfit2 = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평가금액"));
                    double profitRate = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)"));



                    StockGrid stockGrid = new StockGrid(itemName, amount, buyingPrice, currentPrice, estimateProfit2, profitRate);
                    stockGridList.Add(stockGrid);
                }
                dataGridView2.DataSource = stockGridList;
            }
            //주문정보 추가 코드
            else if(e.sRQName.Equals("주문종목정보"))
            {
                try
                {
                    string 종목명 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim();
                    int 현재가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가"));
                    codeNameLab.Text = 종목명;

                    if (현재가 < 0) 현재가 = -현재가;
                    numericUpDown1.Value = 현재가;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }   
            }
            else if(e.sRQName.Equals("실시간미체결요청")) //필드만들것
            {
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                for(int i=0;i<count;++i)
                {
                    string 주문번호 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName,i, "주문번호").Trim();
                    string 종목코드 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string 종목명 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    int 주문수량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문수량"));
                    int 가격 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "가격"));
                    int 현재가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    int 미체결수량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량"));
                    string 주문구분 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "매매구분").Trim();
                    string 시간 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시간").Trim();

                    outStandingOrderList.Add(new OutStandingOrder(주문번호, 종목코드, 종목명, 주문수량,가격,현재가,미체결수량, 주문구분, 시간));
                }

                dataGridView3.DataSource = outStandingOrderList;
            }
        }
    }
    class StockGrid
    {
        public string 종목명 { get; set; }
        public double 수량 { get; set; }
        public double 매수금 { get; set; }
        public double 현재가 { get; set; }
        public double 평가손익 { get; set; }
        public double 수익률 { get; set; }

        public StockGrid() { }

        public StockGrid(string itemName, double amount, double buyingPrice,
                 double currentPrice, double estimateProfit2, double profitRate)
        {

            this.종목명 = itemName;
            this.수량 = amount;
            this.매수금 = buyingPrice;
            this.현재가 = currentPrice;
            this.평가손익 = estimateProfit2;
            this.수익률 = profitRate;
        }

    }
    class OutStandingOrder
    {
        public string 주문번호 { get; set; }
        public string 종목코드 { get; set; }
        public string 종목명 { get; set; }
        public int 주문수량 { get; set; }
        public int 주문가격 { get; set; }
        public int 현재가 { get; set; }
        public int 미체결수량 { get; set; }
        public string 주문구분 { get; set; }
        public string 시간 { get; set; }
        
        //public string 주문번호 { get; set; }
        //public string 주문번호 { get; set; }

        public OutStandingOrder() { }
        public OutStandingOrder(string 주문번호, string 종목코드, string 종목명, int 주문수량, int 주문가격
            , int 현재가, int 미체결수량, string 주문구분, string 시간)
        {
            this.주문번호 = 주문번호;
            this.종목코드 = 종목코드;
            this.종목명 = 종목명;
            this.주문수량 = 주문수량;
            this.주문가격 = 주문가격;
            this.현재가 = 현재가;
            this.미체결수량 = 미체결수량;
            this.주문구분 = 주문구분;
            this.시간 = 시간;

        }

    }
}
