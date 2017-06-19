# Query Migration Tool for Microsoft SQL Server to SAP HANA

The Query Migration Tool for Microsoft SQL Server to SAP HANA is a semi-automatic tool that helps convert most of the data-definition language (DDL) and data-manipulation language (DML).
It will help you to convert structured query language (SQL) in the Microsoft SQL Server database (using T-SQL grammar) to SQL that can be used in the SAP HANA™ database (using ANSI-SQL grammar).

After the conversion, you must check whether the converted version is correct according to your needs. 

This tool supports most of the official T-SQL grammar, and some well-known and widely-used undocumented feature. For more information about the official T-SQL grammar, see the MSDN Library. 

If SAP HANA does not support certain SQL, this tool will do the following: 

• Find equivalents in the SAP HANA database and convert the SQL.

• Delete the SQL in the input file and display relevant comments in the output file.

• Leave the SQL in the input file as it is, for example, the WITH statement.

In order to find out more details please follow the details on the SAP community blog https://blogs.sap.com/2013/04/10/how-to-convert-sql-from-ms-sql-server-to-sap-hana/.

## Prerequisites

The provided source code is a .NET solution. You will need Microsoft Visual Studio installed in your own environment to be able to recompile the provided source code.

## License

 +There is no guaranty or support on the provided source code.

 +The provided source code might use external frameworks and libraries, pay attention if you are building a product that you have the required licenses.

The Query Migration Tool for Microsoft SQL Server to SAP HANA is released under the terms of the MIT license. See LICENSE for more information or see https://opensource.org/licenses/MIT.

## Special thanks

Thanks to the SAP Business One development team for his collaboration on publishing this tool.


