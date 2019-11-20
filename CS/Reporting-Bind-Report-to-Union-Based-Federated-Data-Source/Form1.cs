using System;
using System.Windows.Forms;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Excel;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using DevExpress.XtraReports.UI;

namespace BindReportToFederatedUnionQuery {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        void Button1_Click(object sender, EventArgs e) {
            ReportDesignTool designTool = new ReportDesignTool(CreateReport());
            designTool.ShowRibbonDesignerDialog();
        }
        static FederationDataSource CreateFederationDataSource(SqlDataSource sql, ExcelDataSource excel) {
            // Create a federated query's SQL and Excel sources.
            Source sqlSource = new Source(sql.Name, sql, "Customers");
            Source excelSource = new Source(excel.Name, excel, "");

            // Create a federated Union query.
            var contactsNode = sqlSource.From()
                // Select the "ContactName", "City" and "Phone" columns from the SQL source.
                .Select("ContactName", "City", "Phone")
                .Build()
                // Union the SQL source with the Excel source.
                .Union(excelSource.From()
                    // Select the "ContactName", "City" and "Phone" columns from the Excel source.
                    .Select("ContactName", "City", "Phone").Build(),
                    UnionType.Union)
                // Specify the query's name and build it.
                .Build("Contacts");


            // Create a federated data source and add the federated query to the collection.
            var federationDataSource = new FederationDataSource();
            federationDataSource.Queries.Add(contactsNode);
            // Build the data source schema to display it in the Field List.
            federationDataSource.RebuildResultSchema();

            return federationDataSource;
        }
        public static XtraReport CreateReport() {
            // Create a new report.
            var report = new XtraReport();

            // Create data sources. 
            var sqlDataSource = CreateSqlDataSource();
            var excelDataSource = CreateExcelDataSource();
            var federationDataSource = CreateFederationDataSource(sqlDataSource, excelDataSource);
            // Add all data sources to the report to avoid serialization issues. 
            report.ComponentStorage.AddRange(new IComponent[] { sqlDataSource, excelDataSource, federationDataSource });
            // Assign a federated data source to the report.
            report.DataSource = federationDataSource;
            report.DataMember = "Contacts";

            // Add the Detail band and labels bound to the federated data source's fields.
            var detailBand = new DetailBand() { HeightF = 50 };
            report.Bands.Add(detailBand);
            var contactNameLabel = new XRLabel() { WidthF = 150 };
            var cityLabel = new XRLabel() { WidthF = 150, LocationF = new PointF(200, 0) };
            var phoneLabel = new XRLabel() { WidthF = 200, LocationF = new PointF(400, 0) };
            contactNameLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[ContactName]"));
            cityLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[City]"));
            phoneLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Phone]"));
            detailBand.Controls.AddRange(new[] { contactNameLabel, cityLabel, phoneLabel });

            return report;
        }
        static SqlDataSource CreateSqlDataSource() {
            var connectionParameters = new Access97ConnectionParameters(Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), "Data/nwind.mdb"), "", "");
            var sqlDataSource = new SqlDataSource(connectionParameters) { Name = "Sql_Customers" };
            var categoriesQuery = SelectQueryFluentBuilder.AddTable("Customers").SelectAllColumnsFromTable().Build("Customers");
            sqlDataSource.Queries.Add(categoriesQuery);
            sqlDataSource.RebuildResultSchema();
            return sqlDataSource;
        }
        static ExcelDataSource CreateExcelDataSource() {
            var excelDataSource = new ExcelDataSource() { Name = "Excel_Suppliers" };
            excelDataSource.FileName = Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), "Data/Suppliers.xlsx");
            excelDataSource.SourceOptions = new ExcelSourceOptions() {
                ImportSettings = new ExcelWorksheetSettings("Sheet"),
            };
            excelDataSource.RebuildResultSchema();
            return excelDataSource;
        }
    }
}
