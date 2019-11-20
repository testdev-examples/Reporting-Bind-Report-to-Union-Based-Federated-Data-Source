Imports System
Imports System.Windows.Forms
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.Sql
Imports DevExpress.DataAccess.DataFederation
Imports DevExpress.DataAccess.Excel
Imports System.IO
Imports System.ComponentModel
Imports System.Drawing
Imports DevExpress.XtraReports.UI

Namespace BindReportToFederatedUnionQuery
    Partial Public Class Form1
        Inherits Form

        Public Sub New()
            InitializeComponent()
        End Sub
        Private Sub Button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button1.Click
            Dim designTool As New ReportDesignTool(CreateReport())
            designTool.ShowRibbonDesignerDialog()
        End Sub
        Private Shared Function CreateFederationDataSource(ByVal sql As SqlDataSource, ByVal excel As ExcelDataSource) As FederationDataSource
            ' Create a federated query's SQL and Excel sources.
            Dim sqlSource As New Source(sql.Name, sql, "Customers")
            Dim excelSource As New Source(excel.Name, excel, "")

            ' Create a federated Union query.
            Dim contactsNode = sqlSource.From().Select("ContactName", "City", "Phone").Build().Union(excelSource.From().Select("ContactName", "City", "Phone").Build(), UnionType.Union).Build("Contacts")
                ' Select the "ContactName", "City" and "Phone" columns from the SQL source.
                ' Union the SQL source with the Excel source.
                    ' Select the "ContactName", "City" and "Phone" columns from the Excel source.
                ' Specify the query's name and build it.


            ' Create a federated data source and add the federated query to the collection.
            Dim federationDataSource = New FederationDataSource()
            federationDataSource.Queries.Add(contactsNode)
            ' Build the data source schema to display it in the Field List.
            federationDataSource.RebuildResultSchema()

            Return federationDataSource
        End Function
        Public Shared Function CreateReport() As XtraReport
            ' Create a new report.
            Dim report = New XtraReport()

            ' Create data sources. 
            Dim sqlDataSource = CreateSqlDataSource()
            Dim excelDataSource = CreateExcelDataSource()
            Dim federationDataSource = CreateFederationDataSource(sqlDataSource, excelDataSource)
            ' Add all data sources to the report to avoid serialization issues. 
            report.ComponentStorage.AddRange(New IComponent() { sqlDataSource, excelDataSource, federationDataSource })
            ' Assign a federated data source to the report.
            report.DataSource = federationDataSource
            report.DataMember = "Contacts"

            ' Add the Detail band and labels bound to the federated data source's fields.
            Dim detailBand = New DetailBand() With {.HeightF = 50}
            report.Bands.Add(detailBand)
            Dim contactNameLabel = New XRLabel() With {.WidthF = 150}
            Dim cityLabel = New XRLabel() With { _
                .WidthF = 150, _
                .LocationF = New PointF(200, 0) _
            }
            Dim phoneLabel = New XRLabel() With { _
                .WidthF = 200, _
                .LocationF = New PointF(400, 0) _
            }
            contactNameLabel.ExpressionBindings.Add(New ExpressionBinding("BeforePrint", "Text", "[ContactName]"))
            cityLabel.ExpressionBindings.Add(New ExpressionBinding("BeforePrint", "Text", "[City]"))
            phoneLabel.ExpressionBindings.Add(New ExpressionBinding("BeforePrint", "Text", "[Phone]"))
            detailBand.Controls.AddRange( { contactNameLabel, cityLabel, phoneLabel })

            Return report
        End Function
        Private Shared Function CreateSqlDataSource() As SqlDataSource
            Dim connectionParameters = New Access97ConnectionParameters(Path.Combine(Path.GetDirectoryName(GetType(Form1).Assembly.Location), "Data/nwind.mdb"), "", "")
            Dim sqlDataSource = New SqlDataSource(connectionParameters) With {.Name = "Sql_Customers"}
            Dim categoriesQuery = SelectQueryFluentBuilder.AddTable("Customers").SelectAllColumnsFromTable().Build("Customers")
            sqlDataSource.Queries.Add(categoriesQuery)
            sqlDataSource.RebuildResultSchema()
            Return sqlDataSource
        End Function
        Private Shared Function CreateExcelDataSource() As ExcelDataSource
            Dim excelDataSource = New ExcelDataSource() With {.Name = "Excel_Suppliers"}
            excelDataSource.FileName = Path.Combine(Path.GetDirectoryName(GetType(Form1).Assembly.Location), "Data/Suppliers.xlsx")
            excelDataSource.SourceOptions = New ExcelSourceOptions() With {.ImportSettings = New ExcelWorksheetSettings("Sheet")}
            excelDataSource.RebuildResultSchema()
            Return excelDataSource
        End Function
    End Class
End Namespace
