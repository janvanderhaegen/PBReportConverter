namespace ReportMigration.Reports
{
    partial class XtraReport2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.DataAccess.Sql.CustomSqlQuery customSqlQuery1 = new DevExpress.DataAccess.Sql.CustomSqlQuery();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter1 = new DevExpress.DataAccess.Sql.QueryParameter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XtraReport2));
            this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.pageInfo1 = new DevExpress.XtraReports.UI.XRPageInfo();
            this.pageInfo2 = new DevExpress.XtraReports.UI.XRPageInfo();
            this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.label1 = new DevExpress.XtraReports.UI.XRLabel();
            this.VerticalHeader = new DevExpress.XtraReports.UI.VerticalHeaderBand();
            this.table1 = new DevExpress.XtraReports.UI.XRTable();
            this.tableRow1 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell1 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow2 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell2 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow3 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell3 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow4 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell4 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow5 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell5 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow6 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell6 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow7 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell7 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow8 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell8 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow9 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell9 = new DevExpress.XtraReports.UI.XRTableCell();
            this.VerticalDetail = new DevExpress.XtraReports.UI.VerticalDetailBand();
            this.table2 = new DevExpress.XtraReports.UI.XRTable();
            this.tableRow10 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell10 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow11 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell11 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow12 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell12 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow13 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell13 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow14 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell14 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow15 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell15 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow16 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell16 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow17 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell17 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableRow18 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell18 = new DevExpress.XtraReports.UI.XRTableCell();
            this.Title = new DevExpress.XtraReports.UI.XRControlStyle();
            this.PageInfo = new DevExpress.XtraReports.UI.XRControlStyle();
            this.DetailData1Vertical = new DevExpress.XtraReports.UI.XRControlStyle();
            this.DetailData1VerticalFirstRow = new DevExpress.XtraReports.UI.XRControlStyle();
            this.DetailData1VerticalLastRow = new DevExpress.XtraReports.UI.XRControlStyle();
            this.DetailData1VerticalRow_Even = new DevExpress.XtraReports.UI.XRControlStyle();
            this.HeaderData1Vertical = new DevExpress.XtraReports.UI.XRControlStyle();
            this.HeaderData1VerticalFirstRow = new DevExpress.XtraReports.UI.XRControlStyle();
            this.HeaderData1VerticalLastRow = new DevExpress.XtraReports.UI.XRControlStyle();
            this.HeaderData1VerticalRow_Even = new DevExpress.XtraReports.UI.XRControlStyle();
            this.TestParam1 = new DevExpress.XtraReports.Parameters.Parameter();
            ((System.ComponentModel.ISupportInitialize)(this.table1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.table2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // sqlDataSource1
            // 
            this.sqlDataSource1.ConnectionName = "TestDBConnection";
            this.sqlDataSource1.Name = "sqlDataSource1";
            customSqlQuery1.Name = "Query";
            queryParameter1.Name = "StartDate";
            queryParameter1.Type = typeof(global::DevExpress.DataAccess.Expression);
            queryParameter1.Value = new DevExpress.DataAccess.Expression("?TestParam1", typeof(System.DateTime));
            customSqlQuery1.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter1});
            customSqlQuery1.Sql = resources.GetString("customSqlQuery1.Sql");
            this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            customSqlQuery1});
            this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
            // 
            // TopMargin
            // 
            this.TopMargin.Name = "TopMargin";
            // 
            // BottomMargin
            // 
            this.BottomMargin.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.pageInfo1,
            this.pageInfo2});
            this.BottomMargin.Name = "BottomMargin";
            // 
            // pageInfo1
            // 
            this.pageInfo1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.pageInfo1.Name = "pageInfo1";
            this.pageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.DateTime;
            this.pageInfo1.SizeF = new System.Drawing.SizeF(325.5F, 23F);
            this.pageInfo1.StyleName = "PageInfo";
            // 
            // pageInfo2
            // 
            this.pageInfo2.LocationFloat = new DevExpress.Utils.PointFloat(325.5F, 0F);
            this.pageInfo2.Name = "pageInfo2";
            this.pageInfo2.SizeF = new System.Drawing.SizeF(325.5F, 23F);
            this.pageInfo2.StyleName = "PageInfo";
            this.pageInfo2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight;
            this.pageInfo2.TextFormatString = "Page {0} of {1}";
            // 
            // ReportHeader
            // 
            this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.label1});
            this.ReportHeader.HeightF = 60F;
            this.ReportHeader.Name = "ReportHeader";
            // 
            // label1
            // 
            this.label1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.label1.Name = "label1";
            this.label1.SizeF = new System.Drawing.SizeF(114.2927F, 24.19433F);
            this.label1.StyleName = "Title";
            this.label1.Text = "Report Title";
            // 
            // VerticalHeader
            // 
            this.VerticalHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.table1});
            this.VerticalHeader.HeightF = 252F;
            this.VerticalHeader.Name = "VerticalHeader";
            this.VerticalHeader.RepeatEveryPage = true;
            this.VerticalHeader.WidthF = 112.1341F;
            // 
            // table1
            // 
            this.table1.AnchorHorizontal = ((DevExpress.XtraReports.UI.HorizontalAnchorStyles)((DevExpress.XtraReports.UI.HorizontalAnchorStyles.Left | DevExpress.XtraReports.UI.HorizontalAnchorStyles.Right)));
            this.table1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.table1.Name = "table1";
            this.table1.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.tableRow1,
            this.tableRow2,
            this.tableRow3,
            this.tableRow4,
            this.tableRow5,
            this.tableRow6,
            this.tableRow7,
            this.tableRow8,
            this.tableRow9});
            this.table1.SizeF = new System.Drawing.SizeF(112.1341F, 252F);
            // 
            // tableRow1
            // 
            this.tableRow1.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell1});
            this.tableRow1.Name = "tableRow1";
            this.tableRow1.Weight = 0.1111111111111111D;
            // 
            // tableCell1
            // 
            this.tableCell1.Name = "tableCell1";
            this.tableCell1.StyleName = "HeaderData1VerticalFirstRow";
            this.tableCell1.Text = "by pass";
            this.tableCell1.Weight = 1.0248776198823675D;
            this.tableCell1.WordWrap = false;
            // 
            // tableRow2
            // 
            this.tableRow2.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell2});
            this.tableRow2.Name = "tableRow2";
            this.tableRow2.Weight = 0.1111111111111111D;
            // 
            // tableCell2
            // 
            this.tableCell2.Name = "tableCell2";
            this.tableCell2.StyleName = "HeaderData1VerticalRow_Even";
            this.tableCell2.Text = "inlet";
            this.tableCell2.Weight = 0.50523753186597919D;
            this.tableCell2.WordWrap = false;
            // 
            // tableRow3
            // 
            this.tableRow3.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell3});
            this.tableRow3.Name = "tableRow3";
            this.tableRow3.Weight = 0.1111111111111111D;
            // 
            // tableCell3
            // 
            this.tableCell3.Name = "tableCell3";
            this.tableCell3.StyleName = "HeaderData1Vertical";
            this.tableCell3.Text = "whd vol";
            this.tableCell3.Weight = 1.0240594667798486D;
            this.tableCell3.WordWrap = false;
            // 
            // tableRow4
            // 
            this.tableRow4.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell4});
            this.tableRow4.Name = "tableRow4";
            this.tableRow4.Weight = 0.1111111111111111D;
            // 
            // tableCell4
            // 
            this.tableCell4.Name = "tableCell4";
            this.tableCell4.StyleName = "HeaderData1VerticalRow_Even";
            this.tableCell4.Text = "source vol";
            this.tableCell4.Weight = 1.7526766589273437D;
            this.tableCell4.WordWrap = false;
            // 
            // tableRow5
            // 
            this.tableRow5.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell5});
            this.tableRow5.Name = "tableRow5";
            this.tableRow5.Weight = 0.1111111111111111D;
            // 
            // tableCell5
            // 
            this.tableCell5.Name = "tableCell5";
            this.tableCell5.StyleName = "HeaderData1Vertical";
            this.tableCell5.Text = "mcf by pass";
            this.tableCell5.Weight = 2.5242206804277223D;
            this.tableCell5.WordWrap = false;
            // 
            // tableRow6
            // 
            this.tableRow6.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell6});
            this.tableRow6.Name = "tableRow6";
            this.tableRow6.Weight = 0.1111111111111111D;
            // 
            // tableCell6
            // 
            this.tableCell6.Name = "tableCell6";
            this.tableCell6.StyleName = "HeaderData1VerticalRow_Even";
            this.tableCell6.Text = "mcf inlet";
            this.tableCell6.Weight = 1.2014759447871011D;
            this.tableCell6.WordWrap = false;
            // 
            // tableRow7
            // 
            this.tableRow7.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell7});
            this.tableRow7.Name = "tableRow7";
            this.tableRow7.Weight = 0.1111111111111111D;
            // 
            // tableCell7
            // 
            this.tableCell7.Name = "tableCell7";
            this.tableCell7.StyleName = "HeaderData1Vertical";
            this.tableCell7.Text = "mcf whd vol";
            this.tableCell7.Weight = 2.52174306841977D;
            this.tableCell7.WordWrap = false;
            // 
            // tableRow8
            // 
            this.tableRow8.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell8});
            this.tableRow8.Name = "tableRow8";
            this.tableRow8.Weight = 0.1111111111111111D;
            // 
            // tableCell8
            // 
            this.tableCell8.Name = "tableCell8";
            this.tableCell8.StyleName = "HeaderData1VerticalRow_Even";
            this.tableCell8.Text = "mcf source vol";
            this.tableCell8.Weight = 5.5284191758478984D;
            this.tableCell8.WordWrap = false;
            // 
            // tableRow9
            // 
            this.tableRow9.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell9});
            this.tableRow9.Name = "tableRow9";
            this.tableRow9.Weight = 0.1111111111111111D;
            // 
            // tableCell9
            // 
            this.tableCell9.Name = "tableCell9";
            this.tableCell9.StyleName = "HeaderData1VerticalLastRow";
            this.tableCell9.Text = "mmbtu mcf pbase";
            this.tableCell9.Weight = 1D;
            this.tableCell9.WordWrap = false;
            // 
            // VerticalDetail
            // 
            this.VerticalDetail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.table2});
            this.VerticalDetail.HeightF = 252F;
            this.VerticalDetail.KeepTogether = true;
            this.VerticalDetail.Name = "VerticalDetail";
            this.VerticalDetail.WidthF = 112.1341F;
            // 
            // table2
            // 
            this.table2.AnchorHorizontal = ((DevExpress.XtraReports.UI.HorizontalAnchorStyles)((DevExpress.XtraReports.UI.HorizontalAnchorStyles.Left | DevExpress.XtraReports.UI.HorizontalAnchorStyles.Right)));
            this.table2.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.table2.Name = "table2";
            this.table2.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.tableRow10,
            this.tableRow11,
            this.tableRow12,
            this.tableRow13,
            this.tableRow14,
            this.tableRow15,
            this.tableRow16,
            this.tableRow17,
            this.tableRow18});
            this.table2.SizeF = new System.Drawing.SizeF(112.1341F, 252F);
            // 
            // tableRow10
            // 
            this.tableRow10.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell10});
            this.tableRow10.Name = "tableRow10";
            this.tableRow10.Weight = 0.1111111111111111D;
            // 
            // tableCell10
            // 
            this.tableCell10.CanGrow = false;
            this.tableCell10.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[by_pass]")});
            this.tableCell10.Name = "tableCell10";
            this.tableCell10.StyleName = "DetailData1VerticalFirstRow";
            this.tableCell10.StylePriority.UseTextAlignment = false;
            this.tableCell10.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell10.Weight = 0.37577144502984966D;
            this.tableCell10.WordWrap = false;
            // 
            // tableRow11
            // 
            this.tableRow11.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell11});
            this.tableRow11.Name = "tableRow11";
            this.tableRow11.Weight = 0.1111111111111111D;
            // 
            // tableCell11
            // 
            this.tableCell11.CanGrow = false;
            this.tableCell11.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[inlet]")});
            this.tableCell11.Name = "tableCell11";
            this.tableCell11.StyleName = "DetailData1VerticalRow_Even";
            this.tableCell11.StylePriority.UseTextAlignment = false;
            this.tableCell11.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell11.Weight = 0.37577144502984966D;
            this.tableCell11.WordWrap = false;
            // 
            // tableRow12
            // 
            this.tableRow12.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell12});
            this.tableRow12.Name = "tableRow12";
            this.tableRow12.Weight = 0.1111111111111111D;
            // 
            // tableCell12
            // 
            this.tableCell12.CanGrow = false;
            this.tableCell12.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[whd_vol]")});
            this.tableCell12.Name = "tableCell12";
            this.tableCell12.StyleName = "DetailData1Vertical";
            this.tableCell12.StylePriority.UseTextAlignment = false;
            this.tableCell12.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell12.Weight = 0.37577144502984966D;
            this.tableCell12.WordWrap = false;
            // 
            // tableRow13
            // 
            this.tableRow13.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell13});
            this.tableRow13.Name = "tableRow13";
            this.tableRow13.Weight = 0.1111111111111111D;
            // 
            // tableCell13
            // 
            this.tableCell13.CanGrow = false;
            this.tableCell13.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[source_vol]")});
            this.tableCell13.Name = "tableCell13";
            this.tableCell13.StyleName = "DetailData1VerticalRow_Even";
            this.tableCell13.StylePriority.UseTextAlignment = false;
            this.tableCell13.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell13.Weight = 0.37577144502984966D;
            this.tableCell13.WordWrap = false;
            // 
            // tableRow14
            // 
            this.tableRow14.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell14});
            this.tableRow14.Name = "tableRow14";
            this.tableRow14.Weight = 0.1111111111111111D;
            // 
            // tableCell14
            // 
            this.tableCell14.CanGrow = false;
            this.tableCell14.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[mcf_by_pass]")});
            this.tableCell14.Name = "tableCell14";
            this.tableCell14.StyleName = "DetailData1Vertical";
            this.tableCell14.StylePriority.UseTextAlignment = false;
            this.tableCell14.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell14.Weight = 0.37577144502984966D;
            this.tableCell14.WordWrap = false;
            // 
            // tableRow15
            // 
            this.tableRow15.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell15});
            this.tableRow15.Name = "tableRow15";
            this.tableRow15.Weight = 0.1111111111111111D;
            // 
            // tableCell15
            // 
            this.tableCell15.CanGrow = false;
            this.tableCell15.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[mcf_inlet]")});
            this.tableCell15.Name = "tableCell15";
            this.tableCell15.StyleName = "DetailData1VerticalRow_Even";
            this.tableCell15.StylePriority.UseTextAlignment = false;
            this.tableCell15.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell15.Weight = 0.37577144502984966D;
            this.tableCell15.WordWrap = false;
            // 
            // tableRow16
            // 
            this.tableRow16.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell16});
            this.tableRow16.Name = "tableRow16";
            this.tableRow16.Weight = 0.1111111111111111D;
            // 
            // tableCell16
            // 
            this.tableCell16.CanGrow = false;
            this.tableCell16.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[mcf_whd_vol]")});
            this.tableCell16.Name = "tableCell16";
            this.tableCell16.StyleName = "DetailData1Vertical";
            this.tableCell16.StylePriority.UseTextAlignment = false;
            this.tableCell16.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell16.Weight = 0.37577144502984966D;
            this.tableCell16.WordWrap = false;
            // 
            // tableRow17
            // 
            this.tableRow17.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell17});
            this.tableRow17.Name = "tableRow17";
            this.tableRow17.Weight = 0.1111111111111111D;
            // 
            // tableCell17
            // 
            this.tableCell17.CanGrow = false;
            this.tableCell17.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[mcf_source_vol]")});
            this.tableCell17.Name = "tableCell17";
            this.tableCell17.StyleName = "DetailData1VerticalRow_Even";
            this.tableCell17.StylePriority.UseTextAlignment = false;
            this.tableCell17.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell17.Weight = 0.37577144502984966D;
            this.tableCell17.WordWrap = false;
            // 
            // tableRow18
            // 
            this.tableRow18.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell18});
            this.tableRow18.Name = "tableRow18";
            this.tableRow18.Weight = 0.1111111111111111D;
            // 
            // tableCell18
            // 
            this.tableCell18.CanGrow = false;
            this.tableCell18.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[mmbtu_mcf_pbase]")});
            this.tableCell18.Name = "tableCell18";
            this.tableCell18.StyleName = "DetailData1VerticalLastRow";
            this.tableCell18.StylePriority.UseTextAlignment = false;
            this.tableCell18.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.tableCell18.Weight = 0.37577144502984966D;
            this.tableCell18.WordWrap = false;
            // 
            // Title
            // 
            this.Title.BackColor = System.Drawing.Color.Transparent;
            this.Title.BorderColor = System.Drawing.Color.Black;
            this.Title.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.Title.BorderWidth = 1F;
            this.Title.Font = new DevExpress.Drawing.DXFont("Arial", 14.25F);
            this.Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.Title.Name = "Title";
            this.Title.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            // 
            // PageInfo
            // 
            this.PageInfo.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F, DevExpress.Drawing.DXFontStyle.Bold);
            this.PageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.PageInfo.Name = "PageInfo";
            this.PageInfo.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            // 
            // DetailData1Vertical
            // 
            this.DetailData1Vertical.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
            this.DetailData1Vertical.BorderColor = System.Drawing.Color.White;
            this.DetailData1Vertical.Borders = DevExpress.XtraPrinting.BorderSide.Right;
            this.DetailData1Vertical.BorderWidth = 2F;
            this.DetailData1Vertical.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F);
            this.DetailData1Vertical.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.DetailData1Vertical.Name = "DetailData1Vertical";
            this.DetailData1Vertical.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.DetailData1Vertical.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // DetailData1VerticalFirstRow
            // 
            this.DetailData1VerticalFirstRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
            this.DetailData1VerticalFirstRow.BorderColor = System.Drawing.Color.White;
            this.DetailData1VerticalFirstRow.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right)));
            this.DetailData1VerticalFirstRow.BorderWidth = 2F;
            this.DetailData1VerticalFirstRow.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F);
            this.DetailData1VerticalFirstRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.DetailData1VerticalFirstRow.Name = "DetailData1VerticalFirstRow";
            this.DetailData1VerticalFirstRow.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.DetailData1VerticalFirstRow.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // DetailData1VerticalLastRow
            // 
            this.DetailData1VerticalLastRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
            this.DetailData1VerticalLastRow.BorderColor = System.Drawing.Color.White;
            this.DetailData1VerticalLastRow.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.DetailData1VerticalLastRow.BorderWidth = 2F;
            this.DetailData1VerticalLastRow.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F);
            this.DetailData1VerticalLastRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.DetailData1VerticalLastRow.Name = "DetailData1VerticalLastRow";
            this.DetailData1VerticalLastRow.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.DetailData1VerticalLastRow.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // DetailData1VerticalRow_Even
            // 
            this.DetailData1VerticalRow_Even.BackColor = System.Drawing.Color.Transparent;
            this.DetailData1VerticalRow_Even.BorderColor = System.Drawing.Color.White;
            this.DetailData1VerticalRow_Even.Borders = DevExpress.XtraPrinting.BorderSide.Right;
            this.DetailData1VerticalRow_Even.BorderWidth = 2F;
            this.DetailData1VerticalRow_Even.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F);
            this.DetailData1VerticalRow_Even.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.DetailData1VerticalRow_Even.Name = "DetailData1VerticalRow_Even";
            this.DetailData1VerticalRow_Even.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.DetailData1VerticalRow_Even.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // HeaderData1Vertical
            // 
            this.HeaderData1Vertical.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(111)))), ((int)(((byte)(111)))));
            this.HeaderData1Vertical.BorderColor = System.Drawing.Color.White;
            this.HeaderData1Vertical.Borders = DevExpress.XtraPrinting.BorderSide.Right;
            this.HeaderData1Vertical.BorderWidth = 2F;
            this.HeaderData1Vertical.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F, DevExpress.Drawing.DXFontStyle.Bold);
            this.HeaderData1Vertical.ForeColor = System.Drawing.Color.White;
            this.HeaderData1Vertical.Name = "HeaderData1Vertical";
            this.HeaderData1Vertical.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.HeaderData1Vertical.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // HeaderData1VerticalFirstRow
            // 
            this.HeaderData1VerticalFirstRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(111)))), ((int)(((byte)(111)))));
            this.HeaderData1VerticalFirstRow.BorderColor = System.Drawing.Color.White;
            this.HeaderData1VerticalFirstRow.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right)));
            this.HeaderData1VerticalFirstRow.BorderWidth = 2F;
            this.HeaderData1VerticalFirstRow.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F, DevExpress.Drawing.DXFontStyle.Bold);
            this.HeaderData1VerticalFirstRow.ForeColor = System.Drawing.Color.White;
            this.HeaderData1VerticalFirstRow.Name = "HeaderData1VerticalFirstRow";
            this.HeaderData1VerticalFirstRow.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.HeaderData1VerticalFirstRow.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // HeaderData1VerticalLastRow
            // 
            this.HeaderData1VerticalLastRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(111)))), ((int)(((byte)(111)))));
            this.HeaderData1VerticalLastRow.BorderColor = System.Drawing.Color.White;
            this.HeaderData1VerticalLastRow.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.HeaderData1VerticalLastRow.BorderWidth = 2F;
            this.HeaderData1VerticalLastRow.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F, DevExpress.Drawing.DXFontStyle.Bold);
            this.HeaderData1VerticalLastRow.ForeColor = System.Drawing.Color.White;
            this.HeaderData1VerticalLastRow.Name = "HeaderData1VerticalLastRow";
            this.HeaderData1VerticalLastRow.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.HeaderData1VerticalLastRow.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // HeaderData1VerticalRow_Even
            // 
            this.HeaderData1VerticalRow_Even.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(131)))), ((int)(((byte)(131)))), ((int)(((byte)(131)))));
            this.HeaderData1VerticalRow_Even.BorderColor = System.Drawing.Color.White;
            this.HeaderData1VerticalRow_Even.Borders = DevExpress.XtraPrinting.BorderSide.Right;
            this.HeaderData1VerticalRow_Even.BorderWidth = 2F;
            this.HeaderData1VerticalRow_Even.Font = new DevExpress.Drawing.DXFont("Arial", 8.25F, DevExpress.Drawing.DXFontStyle.Bold);
            this.HeaderData1VerticalRow_Even.ForeColor = System.Drawing.Color.White;
            this.HeaderData1VerticalRow_Even.Name = "HeaderData1VerticalRow_Even";
            this.HeaderData1VerticalRow_Even.Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0, 100F);
            this.HeaderData1VerticalRow_Even.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // TestParam1
            // 
            this.TestParam1.Description = "Parameter1";
            this.TestParam1.Name = "TestParam1";
            this.TestParam1.Type = typeof(global::System.DateTime);
            this.TestParam1.ValueInfo = "2010-06-01";
            // 
            // XtraReport2
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.ReportHeader,
            this.VerticalHeader,
            this.VerticalDetail});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
            this.DataMember = "Query";
            this.DataSource = this.sqlDataSource1;
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Margins = new DevExpress.Drawing.DXMargins(100F, 99F, 100F, 100F);
            this.ParameterPanelLayoutItems.AddRange(new DevExpress.XtraReports.Parameters.ParameterPanelLayoutItem[] {
            new DevExpress.XtraReports.Parameters.ParameterLayoutItem(this.TestParam1, DevExpress.XtraReports.Parameters.Orientation.Horizontal)});
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.TestParam1});
            this.StyleSheet.AddRange(new DevExpress.XtraReports.UI.XRControlStyle[] {
            this.Title,
            this.PageInfo,
            this.DetailData1Vertical,
            this.DetailData1VerticalFirstRow,
            this.DetailData1VerticalLastRow,
            this.DetailData1VerticalRow_Even,
            this.HeaderData1Vertical,
            this.HeaderData1VerticalFirstRow,
            this.HeaderData1VerticalLastRow,
            this.HeaderData1VerticalRow_Even});
            this.Version = "24.1";
            this.VerticalContentSplitting = DevExpress.XtraPrinting.VerticalContentSplitting.Smart;
            ((System.ComponentModel.ISupportInitialize)(this.table1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.table2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.XRPageInfo pageInfo1;
        private DevExpress.XtraReports.UI.XRPageInfo pageInfo2;
        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.XtraReports.UI.XRLabel label1;
        private DevExpress.XtraReports.UI.VerticalHeaderBand VerticalHeader;
        private DevExpress.XtraReports.UI.XRTable table1;
        private DevExpress.XtraReports.UI.XRTableRow tableRow1;
        private DevExpress.XtraReports.UI.XRTableCell tableCell1;
        private DevExpress.XtraReports.UI.XRTableRow tableRow2;
        private DevExpress.XtraReports.UI.XRTableCell tableCell2;
        private DevExpress.XtraReports.UI.XRTableRow tableRow3;
        private DevExpress.XtraReports.UI.XRTableCell tableCell3;
        private DevExpress.XtraReports.UI.XRTableRow tableRow4;
        private DevExpress.XtraReports.UI.XRTableCell tableCell4;
        private DevExpress.XtraReports.UI.XRTableRow tableRow5;
        private DevExpress.XtraReports.UI.XRTableCell tableCell5;
        private DevExpress.XtraReports.UI.XRTableRow tableRow6;
        private DevExpress.XtraReports.UI.XRTableCell tableCell6;
        private DevExpress.XtraReports.UI.XRTableRow tableRow7;
        private DevExpress.XtraReports.UI.XRTableCell tableCell7;
        private DevExpress.XtraReports.UI.XRTableRow tableRow8;
        private DevExpress.XtraReports.UI.XRTableCell tableCell8;
        private DevExpress.XtraReports.UI.XRTableRow tableRow9;
        private DevExpress.XtraReports.UI.XRTableCell tableCell9;
        private DevExpress.XtraReports.UI.VerticalDetailBand VerticalDetail;
        private DevExpress.XtraReports.UI.XRTable table2;
        private DevExpress.XtraReports.UI.XRTableRow tableRow10;
        private DevExpress.XtraReports.UI.XRTableCell tableCell10;
        private DevExpress.XtraReports.UI.XRTableRow tableRow11;
        private DevExpress.XtraReports.UI.XRTableCell tableCell11;
        private DevExpress.XtraReports.UI.XRTableRow tableRow12;
        private DevExpress.XtraReports.UI.XRTableCell tableCell12;
        private DevExpress.XtraReports.UI.XRTableRow tableRow13;
        private DevExpress.XtraReports.UI.XRTableCell tableCell13;
        private DevExpress.XtraReports.UI.XRTableRow tableRow14;
        private DevExpress.XtraReports.UI.XRTableCell tableCell14;
        private DevExpress.XtraReports.UI.XRTableRow tableRow15;
        private DevExpress.XtraReports.UI.XRTableCell tableCell15;
        private DevExpress.XtraReports.UI.XRTableRow tableRow16;
        private DevExpress.XtraReports.UI.XRTableCell tableCell16;
        private DevExpress.XtraReports.UI.XRTableRow tableRow17;
        private DevExpress.XtraReports.UI.XRTableCell tableCell17;
        private DevExpress.XtraReports.UI.XRTableRow tableRow18;
        private DevExpress.XtraReports.UI.XRTableCell tableCell18;
        private DevExpress.XtraReports.UI.XRControlStyle Title;
        private DevExpress.XtraReports.UI.XRControlStyle PageInfo;
        private DevExpress.XtraReports.UI.XRControlStyle DetailData1Vertical;
        private DevExpress.XtraReports.UI.XRControlStyle DetailData1VerticalFirstRow;
        private DevExpress.XtraReports.UI.XRControlStyle DetailData1VerticalLastRow;
        private DevExpress.XtraReports.UI.XRControlStyle DetailData1VerticalRow_Even;
        private DevExpress.XtraReports.UI.XRControlStyle HeaderData1Vertical;
        private DevExpress.XtraReports.UI.XRControlStyle HeaderData1VerticalFirstRow;
        private DevExpress.XtraReports.UI.XRControlStyle HeaderData1VerticalLastRow;
        private DevExpress.XtraReports.UI.XRControlStyle HeaderData1VerticalRow_Even;
        private DevExpress.XtraReports.Parameters.Parameter TestParam1;
    }
}
