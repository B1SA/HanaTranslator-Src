grammar TransactSql;

options
{
    language = CSharp3;
}

@lexer::namespace {Translator}
@parser::namespace {Translator}

@lexer::header
{
    // 'type' does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute.
#   pragma warning disable 3021
}

@parser::header
{
    // 'type' does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute.
#   pragma warning disable 3021

    using System;
}

@members
{
    public override void DisplayRecognitionError(string[] tokenNames, RecognitionException e)
    {
        Console.WriteLine(String.Format(ResStr.MSG_STATEMENT_NOT_SUPPORTED, 
            GetErrorHeader(e), GetErrorMessage(e, tokenNames)));
    }
}

public
sql returns [List<Statement> res]
        // To make sure there is always a statement where comments can be attached, we will create
        // a dummy "start of sql" statement immediately.
    :   {
            $res = new List<Statement> { new SqlStartStatement() };
        }
        batches
        {
            $res.AddRange($batches.res);
        }
    ;

batches returns [List<Statement> res]
    :   b1=batch { $res = $b1.res; }
        (
                (
                        gs1=goStatement SEMICOLON* { $res.Add($gs1.res); }
                )+
                b2=batch { $res.AddRange($b2.res); }
        )*
        (
                gs2=goStatement SEMICOLON* { $res.Add($gs2.res); }
        )*
        EOF
    ;

batch returns [List<Statement> res]
        // Some statements have to be alone in one batch (like CREATE PROCEDURE).
    :   batchStatement { $res = new List<Statement> { $batchStatement.res }; }
        // Some statements can be together in a batch with other statements.
    |   simpleStatements { $res = $simpleStatements.res; }
    ;

batchStatement returns [Statement res]
    :   createViewStatement SEMICOLON* { $res = $createViewStatement.res; }
    |   alterViewStatement SEMICOLON* { $res = $alterViewStatement.res; }
    |   createProcedureStatement { $res = $createProcedureStatement.res; }
    |   alterProcedureStatement { $res = $alterProcedureStatement.res; }
    |   createDmlTriggerStatement { $res =  $createDmlTriggerStatement.res; }
	|	createFunctionStatement { $res = $createFunctionStatement.res; }
    ;

simpleStatements returns [List<Statement> res]
    :   ss1=simpleStatement SEMICOLON* { $res = new List<Statement> { $ss1.res }; }
        (ss2=simpleStatement SEMICOLON* { $res.Add($ss2.res); })*
    ;

simpleStatement returns [Statement res]
    :   withSupportingStatement { $res = $withSupportingStatement.res; }
	|	updateStatisticsStatement { $res = $updateStatisticsStatement.res; }
    |   truncateTableStatement { $res = $truncateTableStatement.res; }
    |   executeStatement { $res = $executeStatement.res; }
    |   createTableStatement { $res = $createTableStatement.res; }
    |   alterTableStatement { $res = $alterTableStatement.res; }
    |   dropTableStatement { $res = $dropTableStatement.res; }
    |   createIndexStatement { $res = $createIndexStatement.res; }
    |   alterIndexStatement { $res = $alterIndexStatement.res; }
    |   dropIndexStatement { $res = $dropIndexStatement.res; }
    |   dropViewStatement { $res = $dropViewStatement.res; }
    |   dropProcedureStatement { $res = $dropProcedureStatement.res; }
    |   ifStatement { $res = $ifStatement.res; }
    |   whileStatement { $res = $whileStatement.res; }
    |   breakStatement { $res = $breakStatement.res; }
    |   continueStatement { $res = $continueStatement.res; }
    |   gotoStatement { $res = $gotoStatement.res; }
    |   labelStatement { $res = $labelStatement.res; }
    |   waitForStatement { $res = $waitForStatement.res; }
    |   tryStatement { $res = $tryStatement.res; }
    |   throwStatement { $res = $throwStatement.res; }
    |   returnStatement { $res = $returnStatement.res; }
    |   blockStatement { $res = $blockStatement.res; }
    |   declareStatement { $res = $declareStatement.res; }
    |   setStatement { $res = $setStatement.res; }
    |   useStatement { $res = $useStatement.res; }
    |   printStatement { $res = $printStatement.res; }
    |   beginTransactionStatement { $res = $beginTransactionStatement.res; }
    |   commitTransactionStatement { $res = $commitTransactionStatement.res; }
    |   rollbackTransactionStatement { $res = $rollbackTransactionStatement.res; }
    |   saveTransactionStatement { $res = $saveTransactionStatement.res; }
    |   createTypeStatement { $res = $createTypeStatement.res;}
    |   dropTypeStatement {$res = $dropTypeStatement.res;}
    |   cursorDeclaration {$res = $cursorDeclaration.res;}
    |   fetchCursorStatement {$res = $fetchCursorStatement.res;}
    |   openCursorStatement {$res = $openCursorStatement.res; }
    |   deallocateStatement {$res = $deallocateStatement.res;}
    |   closeStatement {$res = $closeStatement.res;}
    |   dropTriggerStatement {$res = $dropTriggerStatement.res;}
    |   raisErrorStatement {$res = $raisErrorStatement.res;}
    ;

////////////////////////////////////////////////////////////////////////
// UPDATE STATISTICS table_or_indexed_view_name 
//    [ 
//        { 
//            { index_or_statistics__name }
//          | ( { index_or_statistics_name } [ ,...n ] ) 
//                }
//    ] 
//    [    WITH 
//        [ 
//            FULLSCAN 
//            | SAMPLE number { PERCENT | ROWS } 
//            | RESAMPLE 
//            | <update_stats_stream_option> [ ,...n ]
//        ] 
//        [ [ , ] [ ALL | COLUMNS | INDEX ] 
//        [ [ , ] NORECOMPUTE ] 
//    ] ;
//
// TODO: This is skipped in MSDN, probably not supported anymore
// <update_stats_stream_option> ::=
//    [ STATS_STREAM = stats_stream ]
//    [ ROWCOUNT = numeric_constant ]
//    [ PAGECOUNT = numeric_contant ]
//
//////////////////////////////////////////////////////////////////////////

updateStatisticsStatement returns [UpdateStatisticStatement res]
	: UPDATE STATISTICS dbObject identifierOrIdentifiersListInParens? withUpdateStatisticsClause? 
		{ $res = new UpdateStatisticStatement( $dbObject.res, $identifierOrIdentifiersListInParens.res, $withUpdateStatisticsClause.res ); }
	;

withUpdateStatisticsClause returns [WithUpdateStatisticClause res]
	: WITH statisticWithOptions? (COMMA statRangeType)? (COMMA NORECOMPUTE)? 
		{ $res = new WithUpdateStatisticClause($statisticWithOptions.res, $statRangeType.res, $NORECOMPUTE  == null ? false : true); }
	;

statisticWithOptions returns [StatisticsWithOption res]
	: FULLSCAN { $res = new StatisticsWithOption( StatisticsOptionType.FULLSCAN, null); }
	| SAMPLE Integer PERCENT { $res = new StatisticsWithOption( StatisticsOptionType.SAMPLEPERCENT, Int32.Parse($Integer.Text) ); }
	| SAMPLE Integer ROWS { $res = new StatisticsWithOption( StatisticsOptionType.SAMPLEROWS, Int32.Parse($Integer.Text) ); }
	| RESAMPLE { $res = new StatisticsWithOption( StatisticsOptionType.RESAMPLE, null); }
	// TODO (see note in UPDATE STATISTIC comment): updateStatsStreamOption
	;

statRangeType returns [StatisticRangeType res]
	: ALL { $res = StatisticRangeType.ALL; }
	| COLUMNS { $res = StatisticRangeType.COLUMNS; }
	| INDEX { $res = StatisticRangeType.INDEX; }
	;

withSupportingStatement returns [Statement res]
    :   withClause?
        (
                updateStatement { $updateStatement.res.WithClause = $withClause.res; $res = $updateStatement.res; }
            |   insertStatement { $insertStatement.res.WithClause = $withClause.res; $res = $insertStatement.res; }
            |   deleteStatement { $deleteStatement.res.WithClause = $withClause.res; $res = $deleteStatement.res; }
            |   selectStatement { $selectStatement.res.WithClause = $withClause.res; $res = $selectStatement.res; }
        )
    ;

withClause returns [List<WithCommonTable> res]
    :   WITH
        wct1=withCommonTable { $res = new List<WithCommonTable> { $wct1.res }; }
        (COMMA wct2=withCommonTable { $res.Add($wct2.res); })*
    ;

withCommonTable returns [WithCommonTable res]
    :   identifier identifierListInParens? AS LPAREN queryExpression RPAREN
        {
            $res = new WithCommonTable($identifier.res, $identifierListInParens.res, $queryExpression.res);
        }
    ;

// UPDATE

/*/////////////////////////////////////////////////////////////////////////////
//[ WITH <common_table_expression> [...n] ]
//UPDATE 
//    [ TOP ( expression ) [ PERCENT ] ] 
//    { { table_alias | <object> | rowset_function_limited 
//         [ WITH ( <Table_Hint_Limited> [ ...n ] ) ]
//      }
//      | @table_variable    
//    }
//    SET
//        { column_name = { expression | DEFAULT | NULL }
//          | { udt_column_name.{ { property_name = expression
//                                | field_name = expression }
//                                | method_name ( argument [ ,...n ] )
//                              }
//          }
//          | column_name { .WRITE ( expression , @Offset , @Length ) }
//          | @variable = expression
//          | @variable = column = expression
//          | column_name { += | -= | *= | /= | %= | &= | ^= | |= } expression
//          | @variable { += | -= | *= | /= | %= | &= | ^= | |= } expression
//          | @variable = column { += | -= | *= | /= | %= | &= | ^= | |= } expression
//        } [ ,...n ] 
//
//    [ <OUTPUT Clause> ]
//    [ FROM{ <table_source> } [ ,...n ] ] 
//    [ WHERE { <search_condition> 
//            | { [ CURRENT OF 
//                  { { [ GLOBAL ] cursor_name } 
//                      | cursor_variable_name 
//                  } 
//                ]
//              }
//            } 
//    ] 
//    [ OPTION ( <query_hint> [ ,...n ] ) ]
//[ ; ]
/*/////////////////////////////////////////////////////////////////////////////

updateStatement returns [UpdateStatement res]
	:  UPDATE topClause? tableSubSource SET setClause outputClause? fromClause? whereClauseSupportingCursor? optionClause?
        {
            $res = new UpdateStatement($topClause.res, $tableSubSource.res, $setClause.res, $outputClause.res,
                $fromClause.res, $whereClauseSupportingCursor.res, $optionClause.res);
        }
    ;

/*/////////////////////////////////////////////////////////////////////////////
//<OUTPUT_CLAUSE> ::=
//{
//    [ OUTPUT <dml_select_list> INTO { @table_variable | output_table } [ ( column_list ) ] ]
//    [ OUTPUT <dml_select_list> ]
//}
//<dml_select_list> ::=
//{ <column_name> | scalar_expression } [ [AS] column_alias_identifier ]
//    [ ,...n ]
//
//<column_name> ::=
//{ DELETED | INSERTED | from_table_name } . { * | column_name }
//    | $action
//
//Definition <dml_select_list> has been rewriten to be less strict :
//<dml_select_list> ::=
//{ expression } [ [AS] column_alias_idenfier }
// | $action
// [, ...n]
//
//(In original version aggregate functions are not allowed in dml_select_list expressions)
/*/////////////////////////////////////////////////////////////////////////////

outputClause returns [OutputClause res]
    :   OUTPUT outputSelectList outputClauseInto? { $res = new OutputClause($outputSelectList.res, $outputClauseInto.res); }
    ;

outputSelectList returns [List<SelectItem> res]
    :   osi1=outputSelectItem { $res = new List<SelectItem> { $osi1.res }; }
        (COMMA osi2=outputSelectItem { $res.Add($osi2.res); })*
    ;

outputSelectItem returns [SelectItem res]
    :   (dbObject DOT ASTERISK) => dbObject DOT ASTERISK { $res = new TableWildcardSelectItem($dbObject.res); }
    |   expression (AS? selectAlias)? { $res = new ExpressionSelectItem($expression.res, $selectAlias.res); }
    |   DOLLAR_ACTION (AS? selectAlias)? { $res = new DollarActionSelectItem($selectAlias.res); }
    ;

outputClauseInto returns [OutputClauseInto res]
    :   INTO Variable identifierListInParens? { $res = new VariableOutputClauseInto($Variable.text, $identifierListInParens.res); }
    |   INTO dbObject identifierListInParens? { $res = new TableOutputClauseInto($dbObject.res, $identifierListInParens.res); }
    ;

/*/////////////////////////////////////////////////////////////////////////////
    SET
        { column_name = { expression | DEFAULT | NULL }
          | { udt_column_name.{ { property_name = expression
                                | field_name = expression }
                                | method_name ( argument [ ,...n ] )
                              }
          }
          | column_name { .WRITE ( expression , @Offset , @Length ) }
          | column_name { += | -= | *= | /= | %= | &= | ^= | |= } expression
          | @variable = expression
          | @variable { += | -= | *= | /= | %= | &= | ^= | |= } expression
          | @variable = column = expression
          | @variable = column { += | -= | *= | /= | %= | &= | ^= | |= } expression
        } [ ,...n ] 
/*/////////////////////////////////////////////////////////////////////////////

setClause returns [List<SetItem> res]
    :   si1=setItem { $res = new List<SetItem> { $si1.res }; }
        (COMMA si2=setItem { $res.Add($si2.res); })*
    ;

setItem returns [SetItem res]
        // Note: No support for udt_column_name (user-defined type column).
    :   dbObject
        (
                // There is a conflict between end of dbObject and DOT WRITE. I tried many "clean solutions",
                // but they were either very complicated or did not work. This predicate is not a clean solution,
                // but it works and is very simple. Feel free to improve.
                {
                    SetColumnAssignmentWrite.IsDotWrite($dbObject.res)
                }? =>
                LPAREN expression COMMA sn1=setNumber COMMA sn2=setNumber RPAREN
                {
                    SetColumnAssignmentWrite.RemoveDotWrite($dbObject.res);
                    $res = new SetItemColumn($dbObject.res, new SetColumnAssignmentWrite($expression.res, $sn1.res, $sn2.res));
                }
            |   setColumnAssignment
                {
                    $res = new SetItemColumn($dbObject.res, $setColumnAssignment.res);
                }
        )
    |   variable setVariableAssignment { $res = new SetItemVariable($variable.res, $setVariableAssignment.res); }
    ;

setColumnAssignment returns [SetAssignment res]
    :   EQUAL defaultSupportingExpression { $res = new SetColumnAssignmentEquals($defaultSupportingExpression.res); }
    |   compoundAssignmentOperator expression { $res = new SetColumnAssignmentOperator($compoundAssignmentOperator.res, $expression.res); }
    ;

setVariableAssignment returns [SetAssignment res]
     :  (EQUAL dbObject) => EQUAL dbObject assignmentOperator expression { $res = new SetVariableColumnAssignment($dbObject.res, $assignmentOperator.res, $expression.res); }
     |  assignmentOperator expression { $res = new SetVariableAssignment($assignmentOperator.res, $expression.res); }
     ;

setNumber returns [SetNumber res]
    :   Integer { $res = new SetNumberInteger(Int32.Parse($Integer.Text)); }
    |   NULL { $res = new SetNumberNull(); }
    ;

// INSERT

/*/////////////////////////////////////////////////////////////////////////////
//[ WITH <common_table_expression> [ ,...n ] ]
//INSERT 
//{
//        [ TOP ( expression ) [ PERCENT ] ] 
//        [ INTO ] 
//        { <object> | rowset_function_limited 
//          [ WITH ( <Table_Hint_Limited> [ ...n ] ) ]
//        }
//    {
//        [ ( column_list ) ] 
//        [ <OUTPUT Clause> ]
//        { VALUES ( { DEFAULT | NULL | expression } [ ,...n ] ) [ ,...n     ] 
//        | derived_table 
//        | execute_statement
//        | <dml_table_source>
//        | DEFAULT VALUES 
//        }
//    }
//}
//[;]
/*/////////////////////////////////////////////////////////////////////////////

insertStatement returns [InsertStatement res]
    :   INSERT topClause? INTO? insertTarget identifierListInParens? outputClause? valuesClause
        {
            $res = new InsertStatement($topClause.res, $insertTarget.res, $identifierListInParens.res,
                $outputClause.res, $valuesClause.res);
        }
    ;

insertTarget returns [InsertTarget res]
    :   dbObject { $res = new DbObjectInsertTarget($dbObject.res); }
    |   Variable { $res = new VariableInsertTarget($Variable.Text); }
    |   insertRowsetFunction mandatoryWithTableHints? { $res = new RowsetFunctionInsertTarget($insertRowsetFunction.res, $mandatoryWithTableHints.res); }
    ;

insertRowsetFunction returns [RowsetFunction res]
    :   openQueryFunction { $res = $openQueryFunction.res; }
    |   openRowsetFunction { $res = $openRowsetFunction.res; }
    ;

/*/////////////////////////////////////////////////////////////////////////////
        { VALUES ( { DEFAULT | NULL | expression } [ ,...n ] ) [ ,...n     ] 
        | derived_table 
        | execute_statement
        | <dml_table_source>
        | DEFAULT VALUES 
        }
/*/////////////////////////////////////////////////////////////////////////////

valuesClause returns [ValuesClause res]
    :   VALUES vr1=valuesRecord { $res = new ValuesClauseValues($vr1.res); }
        (COMMA vr2=valuesRecord { $res.Add($vr2.res); })*
    |   valuesSelectStatement { $res = new ValuesClauseSelect($valuesSelectStatement.res); }
    |   executeStatement { $res = new ValuesClauseExec($executeStatement.res); }
    |   DEFAULT VALUES { $res = new ValuesClauseDefault(); }
    ;

valuesRecord returns [List<Expression> res]
    :   LPAREN
        se1=defaultSupportingExpression { $res = new List<Expression> { $se1.res }; }
        (COMMA se2=defaultSupportingExpression { $res.Add($se2.res); })*
        RPAREN
    ;

// The SELECT statement in valuesClause must start with the SELECT keyword, not with an opening paren.
// Otherwise it will conflict with outputClause and insertColumnList.
// Let's take the string "OUTPUT abc ((SELECT 0 FROM OINV))":
//  - Is it "OUTPUT dbObject selectStatement"?
//  - Or is it "OUTPUT genericScalarFunction"?
// SQL server resolves this by only allowing selectStatement to start with SELECT here.
//
// In grammar this is achieved by a modified selectStatement and queryExpression rule, that only
// allows querySpecification as first part of queryExpression. In normal queryExpression the
// first part can be a full queryExpressionTerm, which also allows parens.

valuesSelectStatement returns [SelectStatement res]
    :   LPAREN? valuesQueryExpression computeClause? forClause? optionClause? RPAREN?
        {
            $res = new SelectStatement($valuesQueryExpression.res,
                $computeClause.res, $forClause.res, $optionClause.res);
        }
    ;

valuesQueryExpression returns [QueryExpression res]
    :   querySpecification { $res = $querySpecification.res; }
        (queryExpressionOperator queryExpressionTerm { $res = new OperatorQueryExpression($res, $queryExpressionOperator.res, $queryExpressionTerm.res); })*
    ;

// DELETE

/*/////////////////////////////////////////////////////////////////////////////
//DELETE
//    [ TOP ( expression ) [ PERCENT ] ] 
//    [ FROM ] 
//    { { table_alias
//      | <object> 
//      | rowset_function_limited 
//      [ WITH ( table_hint_limited [ ...n ] ) ] } 
//      | @table_variable
//    }
//    [ <OUTPUT Clause> ]
//    [ FROM table_source [ ,...n ] ] 
//    [ WHERE { <search_condition> 
//            | { [ CURRENT OF 
//                   { { [ GLOBAL ] cursor_name } 
//                       | cursor_variable_name 
//                   } 
//                ]
//              }
//            } 
//    ] 
//    [ OPTION ( <Query Hint> [ ,...n ] ) ] 
//[; ]
/*/////////////////////////////////////////////////////////////////////////////

deleteStatement returns [DeleteStatement res]
    :   DELETE topClause? FROM? tableSubSource outputClause? fromClause? whereClauseSupportingCursor? optionClause?
        {
            $res = new DeleteStatement($topClause.res, $tableSubSource.res, $outputClause.res,
                $fromClause.res, $whereClauseSupportingCursor.res, $optionClause.res);
        }
    ;

whereClauseSupportingCursor returns [WhereClauseSupportingCursor res]
	: whereClause { $res = new WhereClauseExpression( $whereClause.res ); }
	| WHERE CURRENT OF cursorSource { $res = new WhereClauseCursor( $cursorSource.res); }
	;

// TRUNCATE TABLE

truncateTableStatement returns [TruncateTableStatement res]
    :   TRUNCATE TABLE dbObject { $res = new TruncateTableStatement($dbObject.res); }
    ;

// SELECT

// See also valuesSelectStatement.
selectStatement returns [SelectStatement res]
    :   queryExpression computeClause? forClause? optionClause?
        {
            $res = new SelectStatement($queryExpression.res,  $computeClause.res, $forClause.res, $optionClause.res);
        }
    ;

// See also valuesQueryExpression.
queryExpression returns [QueryExpression res]
    :   qet1=queryExpressionTerm { $res = $qet1.res; }
        (queryExpressionOperator qet2=queryExpressionTerm { $res = new OperatorQueryExpression($res, $queryExpressionOperator.res, $qet2.res); })*
    ;

queryExpressionOperator returns [QueryExpressionOperatorType res]
    :   UNION { $res = QueryExpressionOperatorType.Union; }
    |   UNION ALL { $res = QueryExpressionOperatorType.UnionAll; }
    |   EXCEPT { $res = QueryExpressionOperatorType.Except; }
    |   INTERSECT { $res = QueryExpressionOperatorType.Intersect; }
    ;

queryExpressionTerm returns [QueryExpression res]
    :   querySpecification { $res = $querySpecification.res; }
    |   LPAREN queryExpression RPAREN { $res = $queryExpression.res; }
    ;

querySpecification returns [QuerySpecification res]
    :   selectClause intoClause? fromClause? whereClause? groupByClause? havingClause? orderByClause?
        {
            $res = new QuerySpecification($selectClause.res, $intoClause.res, $fromClause.res, $whereClause.res, $groupByClause.res, $havingClause.res, $orderByClause.res);
        }
    ;

selectClause returns [SelectClause res]
    :   SELECT distinctClause? topClause? selectList
        {
            $res = new SelectClause($distinctClause.res, $topClause.res, $selectList.res);
        }
    ;

distinctClause returns [bool res]
    :   ALL { $res = false; }
    |   DISTINCT { $res = true; }
    ;

topClause returns [TopClause res]
    :   TOP topClauseCount PERCENT? (WITH TIES)?
        {
            $res = new TopClause($topClauseCount.res, $PERCENT != null, $WITH != null);
        }
    ;

topClauseCount returns [Expression res]
        // SQL Server allows more than just Integer here, however allowing a expression without parens
        // creates a conflict between "[TOP Integer + Integer]" and "[TOP Integer] [+Integer]".
    :   Integer { $res = new IntegerConstantExpression(Int32.Parse($Integer.Text)); }
    |   LPAREN expression RPAREN { $res = $expression.res; }
    ;

selectList returns [List<SelectItem> res]
    :   (si1=selectItem { $res = new List<SelectItem> { $si1.res }; })
        (COMMA si2=selectItem { $res.Add($si2.res); })*
    ;

selectItem returns [SelectItem res]
        // Note: Does not support UDT column type.
    :   ASTERISK { $res = new WildcardSelectItem(); }
    |   (variable assignmentOperator expression) => variable assignmentOperator expression { $res = new SelectVariableItem($variable.res, $assignmentOperator.res, $expression.res); }	
    |   (selectAlias EQUAL) => selectAlias EQUAL expression { $res = new ExpressionSelectItem($expression.res, $selectAlias.res); }
    |   (dbObject DOT ASTERISK) => dbObject DOT ASTERISK { $res = new TableWildcardSelectItem($dbObject.res); }
    |   (dbObject DOT selectSpecialColumn) => dbObject DOT selectSpecialColumn { $res = new SpecialColumnSelectItem($selectSpecialColumn.res, $dbObject.res); }
    |   expression (AS? selectAlias)? { $res = new ExpressionSelectItem($expression.res, $selectAlias.res); }
    ;

selectSpecialColumn returns [SelectSpecialColumnType res]
    :   DOLLAR_IDENTITY { $res = SelectSpecialColumnType.Identity; }
    |   DOLLAR_ROWGUID { $res = SelectSpecialColumnType.RowGuid; }
    ;

selectAlias returns [SelectAlias res]
        // Since selectAlias can be at end of a statement, it has to use statementEndIdentifier instead of identifier.
    :   statementEndIdentifier { $res = new IdentifierSelectAlias($statementEndIdentifier.res); }
    |   stringLiteral { $res = new StringLiteralSelectAlias($stringLiteral.res); }
    ;

// INTO, FROM

intoClause returns [DbObject res]
    :   INTO dbObject { $res = $dbObject.res; }
    ;

fromClause returns [List<TableSource> res]
    :   FROM ts1=tableSource { $res = new List<TableSource> { $ts1.res }; }
        (COMMA ts2=tableSource { $res.Add ($ts2.res); })*
    ;

tableSource returns [TableSource res]
    :   tableSubSource { $res = $tableSubSource.res; }
        (joinedTableSource { $joinedTableSource.res.LeftTableSource = $res; $res = $joinedTableSource.res; })*
    ;

joinedTableSource returns [JoinedTableSource res]
    :   CROSS JOIN tableSubSource
        {
            $res = new JoinedTableSource(null, JoinType.Cross, JoinHint.None, $tableSubSource.res, null);
        }
    |   (joinType joinHint?)? JOIN tableSubSource ON expression
        {
            $res = new JoinedTableSource(null, $joinType.res, $joinHint.res, $tableSubSource.res, $expression.res);
        }
    ;

joinType returns [JoinType res]
    :   INNER { $res = JoinType.Inner; }
    |   (
                LEFT { $res = JoinType.Left; }
            |   RIGHT { $res = JoinType.Right; }
            |   FULL { $res = JoinType.Full; }
        ) OUTER?
    ;

joinHint returns [JoinHint res]
    :   LOOP { $res = JoinHint.Loop; }
    |   HASH { $res = JoinHint.Hash; }
    |   MERGE { $res = JoinHint.Merge; }
    |   REMOTE { $res = JoinHint.Remote; }
    ;

tableSubSource returns [TableSource res]
        // Note: Does not support OPENXML, PIVOT, UNPIVOT.
    :   dbObject fromAsAlias? tableSampleClause? optionalWithTableHints?
        {
            $res = new DbObjectTableSource($dbObject.res, $fromAsAlias.res, $tableSampleClause.res, $optionalWithTableHints.res);
        }
    |   rowsetFunction fromAsAlias? identifierListInParens?
        {
            $res = new RowsetFunctionTableSource($rowsetFunction.res, $fromAsAlias.res, $identifierListInParens.res);
        }
    |   LPAREN
        (
                // There is a conflict here that I don't understand.
                // It seems queryExpression starts with SELECT, tableSource starts with dbObject.
                (SELECT) => queryExpression RPAREN faa1=fromAsAlias
                {
                    $res = new SubqueryTableSource($queryExpression.res, $faa1.res);
                }
            |   tableSource RPAREN
                {
                    $res = new NestedParensTableSource($tableSource.res);
                }
            |   // Support a very specific scenario:
                // INSERT INTO changes SELECT stuff FROM (INSERT INTO table VALUES (1, 2, 3) OUTPUT $ACTION, column1, column2)
                // Adding these option here allows nested INSERT/UPDATE/DELETE everywhere, but it is in fact only valid in INSERT.
                insertDmlStatement RPAREN faa2=fromAsAlias identifierListInParens?
                {
                    $res = new InsertDmlTableSource($insertDmlStatement.res, $faa2.res, $identifierListInParens.res);
                }
        )
    |   Variable fromAsAlias?
        {
            $res = new VariableTableSource($Variable.Text, $fromAsAlias.res);
        }
    |   Variable DOT genericScalarFunction fromAsAlias? identifierListInParens?
        {
            $res = new VariableFunctionTableSource($Variable.Text, $genericScalarFunction.res, $fromAsAlias.res, $identifierListInParens.res);
        }
    ;

fromAsAlias returns [Identifier res]
        // Since fromAsAlias can be at end of a statement, it has to use statementEndIdentifier instead of identifier.
    :   AS? statementEndIdentifier { $res = $statementEndIdentifier.res; }
    ;

tableSampleClause returns [TableSampleClause res]
    :   TABLESAMPLE SYSTEM? LPAREN e1=expression percentOrRows? RPAREN (REPEATABLE LPAREN e2=expression RPAREN)?
        {
            $res = new TableSampleClause($e1.res, $percentOrRows.res, $e2.res);
        }
    ;

percentOrRows returns [PercentOrRowsType res]
    :   PERCENT { $res = PercentOrRowsType.Percent; }
    |   ROWS { $res = PercentOrRowsType.Rows; }
    ;

insertDmlStatement returns [Statement res]
    :   insertStatement { $res = $insertStatement.res; }
    |   updateStatement { $res = $updateStatement.res; }
    |   deleteStatement { $res = $deleteStatement.res; }
    ;

// WITH tableHints

mandatoryWithTableHints returns [List<TableHint> res]
    :   WITH LPAREN tableHints RPAREN { $res = $tableHints.res; }
    ;

optionalWithTableHints returns [List<TableHint> res]
    :   WITH? LPAREN tableHints RPAREN { $res = $tableHints.res; }
    ;

tableHints returns [List<TableHint> res]
    :   th1=tableHint { $res = new List<TableHint> { $th1.res }; }
        (COMMA? th2=tableHint { $res.Add($th2.res); })*
    ;

tableHint returns [TableHint res]
    :   indexTableHint { $res = $indexTableHint.res; }
    |   forceSeekTableHint { $res = $forceSeekTableHint.res; }
    |   spatialWindowMaxCellsTableHint { $res = $spatialWindowMaxCellsTableHint.res; }
    |   simpleTableHint { $res = new SimpleTableHint($simpleTableHint.res); }
    ;

indexTableHint returns [IndexTableHint res]
    :   INDEX
        (
                LPAREN indexValueList RPAREN { $res = new IndexTableHint($indexValueList.res); }
            |   EQUAL LPAREN indexValue RPAREN { $res = new IndexTableHint(new List<IndexValue> { $indexValue.res }); }
        )
    ;

indexValueList returns [List<IndexValue> res]
    :   iv1=indexValue { $res = new List<IndexValue> { $iv1.res }; }
        (COMMA iv2=indexValue { $res.Add($iv2.res); })*
    ;

indexValue returns [IndexValue res]
    :   identifier { $res = new IdentifierIndexValue($identifier.res); }
    |   Integer { $res = new IntegerIndexValue(Int32.Parse($Integer.Text)); }
    ;

forceSeekTableHint returns [ForceSeekTableHint res]
    :   FORCESEEK (LPAREN indexValue LPAREN dbObjectList RPAREN RPAREN)?
        {
            $res = new ForceSeekTableHint($indexValue.res, $dbObjectList.res);
        }
    ;

spatialWindowMaxCellsTableHint returns [SpatialWindowMaxCellsTableHint res]
    :   SPATIAL_WINDOW_MAX_CELLS EQUAL Integer { $res = new SpatialWindowMaxCellsTableHint(Int32.Parse($Integer.Text)); }
    ;

simpleTableHint returns [SimpleTableHintType res]
    :   NOEXPAND { $res = SimpleTableHintType.NoExpand; }
    |   FORCESCAN { $res = SimpleTableHintType.ForceScan; }
    |   HOLDLOCK { $res = SimpleTableHintType.HoldLock; }
    |   NOLOCK { $res = SimpleTableHintType.NoLock; }
    |   NOWAIT { $res = SimpleTableHintType.NoWait; }
    |   PAGLOCK { $res = SimpleTableHintType.PagLock; }
    |   READCOMMITTED { $res = SimpleTableHintType.ReadCommitted; }
    |   READCOMMITTEDLOCK { $res = SimpleTableHintType.ReadCommittedLock; }
    |   READPAST { $res = SimpleTableHintType.ReadPast; }
    |   READUNCOMMITTED { $res = SimpleTableHintType.ReadUncommitted; }
    |   REPEATABLEREAD { $res = SimpleTableHintType.RepeatableRead; }
    |   ROWLOCK { $res = SimpleTableHintType.RowLock; }
    |   SERIALIZABLE { $res = SimpleTableHintType.Serializable; }
    |   TABLOCK { $res = SimpleTableHintType.TabLock; }
    |   TABLOCKX { $res = SimpleTableHintType.TabLockX; }
    |   UPDLOCK { $res = SimpleTableHintType.UpdLock; }
    |   XLOCK { $res = SimpleTableHintType.XLock; }
    ;

// WHERE, HAVING

whereClause returns [Expression res]
    :   WHERE expression { $res = $expression.res; }
    ;

orderByClause returns [List<OrderByItem> res]
    :   ORDER BY
        (obi1=orderByItem { $res = new List<OrderByItem> { $obi1.res }; })
        (COMMA obi2=orderByItem { $res.Add($obi2.res); })*
    ;

// GROUP BY

/*/////////////////////////////////////////////////////////////////////////////
GROUP BY <group by spec>

<group by spec> ::=
    <group by item> [ ,...n ]

<group by item> ::=
    <simple group by item>
    | <rollup spec>
    | <cube spec>
    | <grouping sets spec>
    | <grand total>

<rollup spec> ::=
    ROLLUP ( <composite element list> )

<cube spec> ::=
    CUBE ( <composite element list> )

<simple group by item> ::=
    <expression>
/*/////////////////////////////////////////////////////////////////////////////

groupByClause returns [List<GroupByItem> res]
    :   GROUP BY 
        (gbi1=groupByItem { $res = new List<GroupByItem> { $gbi1.res }; })
        (COMMA gbi2=groupByItem { $res.Add($gbi2.res); })*
    ;

groupByItem returns [GroupByItem res]
    :   expression { $res = new ExpressionGroupBy($expression.res); }
    |   groupingSpec { $res = $groupingSpec.res; }
    |   groupingSetsSpec { $res = $groupingSetsSpec.res; }
    |   grandTotal { $res = $grandTotal.res; }
    ;

grandTotal returns [GrandTotal res]
    :   LPAREN RPAREN { $res = new GrandTotal(); }
    ;

groupingSpec returns [GroupingSpec res]
    :   rollupOrCube LPAREN compositeElementList RPAREN
        {
            $res = new GroupingSpec($rollupOrCube.res, $compositeElementList.res);
        }
    ;

rollupOrCube returns [RollupOrCube res]
    :   ROLLUP { $res = RollupOrCube.ROLLUP; }
    |   CUBE { $res = RollupOrCube.CUBE; }
    ;

/*/////////////////////////////////////////////////////////////////////////////
<composite element list> ::=
    <composite element> [ ,...n ]

<composite element> ::=
    <simple group by item>
    | ( <simple group by item list> )

<simple group by item list> ::=
    <simple group by item> [ ,...n ]
/*/////////////////////////////////////////////////////////////////////////////

compositeElement returns [CompositeElement res]
    :   (LPAREN) => LPAREN compositeElementList RPAREN { $res = new CompositeElementList($compositeElementList.res); }
    |   expression { $res = new ExpressionCompositeElement($expression.res); }
    ;

compositeElementList returns [List<CompositeElement> res]
    :   (ce1=compositeElement { $res = new List<CompositeElement> { $ce1.res }; })
        (COMMA ce2=compositeElement { $res.Add($ce2.res); })*
    ;

/*/////////////////////////////////////////////////////////////////////////////
<grouping sets spec> ::=
    GROUPING SETS ( <grouping set list> )

<grouping set list> ::=
    <grouping set> [ ,...n ]

<grouping set> ::=
    <grand total>				// for empty grouping set item list
    | <grouping set item>
    | ( <grouping set item list> )

<grouping set item list> ::=
    <grouping set item> [ ,...n ]
/*/////////////////////////////////////////////////////////////////////////////

groupingSetsSpec returns [GroupingSetSpec res]
    :   GROUPING SETS LPAREN groupingSetList RPAREN { $res = new GroupingSetSpec($groupingSetList.res); }
    ;

groupingSetList returns [List<GroupingSet> res]
    :   (gs1=groupingSet { $res = new List<GroupingSet> { $gs1.res }; })
        (COMMA gs2=groupingSet { $res.Add($gs2.res); })*
    ;

groupingSet returns [GroupingSet res]
    :   (LPAREN) => LPAREN
        (
                RPAREN {$res = new GroupingSetGrandTotal();}
            |   groupingSetItems RPAREN { $res = new GroupingSetItemList($groupingSetItems.res); }
        )
    |   groupingSetItem { $res = $groupingSetItem.res; }
    ;

groupingSetItems returns [List<GroupingSetItem> res]
    :   gsi1=groupingSetItem { $res = new List<GroupingSetItem> { $gsi1.res }; }
        (COMMA gsi2=groupingSetItem { $res.Add($gsi2.res); })*
    ;

/*/////////////////////////////////////////////////////////////////////////////
<grouping set item> ::=
    <simple group by item>
    | <rollup spec>
    | <cube spec>
/*/////////////////////////////////////////////////////////////////////////////

groupingSetItem returns [GroupingSetItem res]
    :   expression { $res = new ExpressionGroupingSetItem($expression.res); }
    |   groupingSpec { $res = new GroupingSpecGroupingSetItem($groupingSpec.res); }
    ;

// HAVING

havingClause returns [Expression res]
    :   HAVING expression { $res = $expression.res; }
    ;

// ORDER BY, COMPUTE, FOR, OPTION

orderByItem returns [OrderByItem res]
    :   expression orderDirection? { $res = new OrderByItem($expression.res, $orderDirection.res); }
    ;

computeClause returns [ComputeClause res]
    :   COMPUTE computeFunctionList (BY computeByList)? { $res = new ComputeClause($computeFunctionList.res, $computeByList.res); }
    ;

computeFunctionList returns [List<ComputeFunction> res]
    :   (cf1=computeFunction { $res = new List<ComputeFunction> { $cf1.res }; })
        (COMMA cf2=computeFunction { $res.Add($cf2.res); })*
    ;

computeFunction returns [ComputeFunction res]
    :   computeFunctionType LPAREN expression RPAREN { $res = new ComputeFunction($computeFunctionType.res, $expression.res); }
    ;

computeFunctionType returns [ComputeFunctionType res]
    :   AVG { $res = ComputeFunctionType.Avg; }
    |   COUNT { $res = ComputeFunctionType.Count; }
    |   MAX { $res = ComputeFunctionType.Max; }
    |   MIN { $res = ComputeFunctionType.Min; }
    |   STDEV { $res = ComputeFunctionType.StDev; }
    |   STDEVP { $res = ComputeFunctionType.StDevP; }
    |   VAR { $res = ComputeFunctionType.Var; }
    |   VARP { $res = ComputeFunctionType.VarP; }
    |   SUM { $res = ComputeFunctionType.Sum; }
    ;

computeByList returns [List<Expression> res]
    :   (e1=expression { $res = new List<Expression> { $e1.res }; })
        (COMMA e2=expression { $res.Add($e2.res); })*
    ;

forClause returns [ForClauseType res]
        // Note: Does not support FOR XML.
    :   FOR BROWSE { $res = ForClauseType.ForBrowse; }
    ;

optionClause returns [List<QueryHint> res]
    :   OPTION LPAREN
        (qh1=queryHint { $res = new List<QueryHint> { $qh1.res }; })
        (COMMA qh2=queryHint { $res.Add($qh2.res); })*
        RPAREN
    ;

queryHint returns [QueryHint res]
    :   (
                HASH { $res = new SimpleQueryHint(SimpleQueryHintType.HashGroup); }
            |   ORDER { $res = new SimpleQueryHint(SimpleQueryHintType.OrderGroup); }
        )
        GROUP
    |   (
                CONCAT { $res = new SimpleQueryHint(SimpleQueryHintType.ConcatUnion); }
            |   HASH { $res = new SimpleQueryHint(SimpleQueryHintType.HashUnion); }
            |   MERGE { $res = new SimpleQueryHint(SimpleQueryHintType.MergeUnion); }
        )
        UNION
    |   (
                LOOP { $res = new SimpleQueryHint(SimpleQueryHintType.LoopJoin); }
            |   MERGE { $res = new SimpleQueryHint(SimpleQueryHintType.MergeJoin); }
            |   HASH { $res = new SimpleQueryHint(SimpleQueryHintType.HashJoin); }
        )
        JOIN
    |   EXPAND VIEWS { $res = new SimpleQueryHint(SimpleQueryHintType.ExpandViews); }
    |   FAST Integer { $res = new FastQueryHint(Int32.Parse($Integer.Text)); }
    |   FORCE ORDER { $res = new SimpleQueryHint(SimpleQueryHintType.ForceOrder); }
    |   IGNORE_NONCLUSTERED_COLUMNSTORE_INDEX { $res = new SimpleQueryHint(SimpleQueryHintType.IgnoreNonclusteredColumnstoreIndex); }
    |   KEEP PLAN { $res = new SimpleQueryHint(SimpleQueryHintType.KeepPlan); }
    |   KEEPFIXED PLAN { $res = new SimpleQueryHint(SimpleQueryHintType.KeepFixedPlan); }
    |   MAXDOP Integer { $res = new MaxDOPQueryHint(Int32.Parse($Integer.Text)); }
    |   MAXRECURSION Integer { $res = new MaxRecursionQueryHint(Int32.Parse($Integer.Text)); }
    |   OPTIMIZE FOR LPAREN optimizeForVariableList RPAREN { $res = new OptimizeForQueryHint($optimizeForVariableList.res); }
    |   OPTIMIZE FOR UNKNOWN { $res = new SimpleQueryHint(SimpleQueryHintType.OptimizeForUnknown); }
    |   PARAMETERIZATION
        (
                SIMPLE { $res = new SimpleQueryHint(SimpleQueryHintType.ParametrizationSimple); }
            |   FORCED { $res = new SimpleQueryHint(SimpleQueryHintType.ParametrizationForced); }
        )
    |   RECOMPILE { $res = new SimpleQueryHint(SimpleQueryHintType.Recompile); }
    |   ROBUST PLAN { $res = new SimpleQueryHint(SimpleQueryHintType.RobustPlan); }
    |   USE PLAN stringLiteral { $res = new UsePlanQueryHint($stringLiteral.res); }
    |   TABLE HINT LPAREN dbObject (COMMA tableHints)? RPAREN { $res = new TableHintsQueryHint($dbObject.res, $tableHints.res); }
    ;

optimizeForVariableList returns [List<OptimizeForVariable> res]
    :   (ofv1=optimizeForVariable { $res = new List<OptimizeForVariable> { $ofv1.res }; })
        (COMMA ofv2=optimizeForVariable { $res.Add($ofv2.res); })*
    ;

optimizeForVariable returns [OptimizeForVariable res]
    :   Variable
        (
                UNKNOWN { $res = new UnknownOptimizeForVariable($Variable.Text); }
            |   EQUAL constant { $res = new ConstantOptimizeForVariable($Variable.Text, $constant.res); }
        )
    ;

// SELECT @local_variable
selectVariableList returns [List<SelectVariableItem> res]
    :   svi1=selectVariableItem { $res = new List<SelectVariableItem> { $svi1.res }; }
        (COMMA svi2=selectVariableItem { $res.Add($svi2.res); })*
    ;

selectVariableItem returns [SelectVariableItem res]
    :   variable assignmentOperator expression { $res = new SelectVariableItem($variable.res, $assignmentOperator.res, $expression.res); }
    ;

// EXEC, EXECUTE

/*/////////////////////////////////////////////////////////////////////////////
[ { EXEC | EXECUTE } ]
    { 
      [ @return_status = ]
      { module_name [ ;number ] | @module_name_var } 
        [ [ @parameter = ] { value 
                           | @variable [ OUTPUT ] 
                           | [ DEFAULT ] 
                           }
        ]
      [ ,...n ]
      [ WITH <execute_option> [ ,...n ] ]
    }
[;]

Execute a character string
{ EXEC | EXECUTE } 
    ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    [ AS { LOGIN | USER } = ' name ' ]
[;]

Execute a pass-through command against a linked server
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ] 'command_string [ ? ]' } [ + ...n ]
        [ { , { value | @variable [ OUTPUT ] } } [ ...n ] ]
    ) 
    [ AS { LOGIN | USER } = ' name ' ]
    [ AT linked_server_name ]
[;]
/*/////////////////////////////////////////////////////////////////////////////

executeStatement returns [ExecStatement res]
    :   executeToken (variable EQUAL)? executeModuleName execSpParams? withExecuteOptions?
        {
            $res = new ExecStatementSP($variable.res, $executeModuleName.res, $execSpParams.res, $withExecuteOptions.res);
        }
    |   executeToken LPAREN expression (COMMA execSqlParams)? RPAREN contextClause? linkedServer?
        {
            $res = new ExecStatementSQL($expression.res, $execSqlParams.res, $contextClause.res, $linkedServer.res);
        }
    ;

executeToken
    :   EXEC
    |   EXECUTE
    ;

executeModuleName returns [ExecModuleName res]
    :   dbObject (SEMICOLON Integer)? { $res = new DbObjectExecModuleName($dbObject.res, $Integer != null ? Int32.Parse($Integer.Text) : -1); }
    |   variable { $res = new VariableExecModuleName($variable.res); }
    ;

execSpParams returns [List<ExecParam> res]
    :   execSpParamList { $res = $execSpParamList.res; }
    |   LPAREN execSpParamList RPAREN { $res = $execSpParamList.res; }
    ;

execSpParamList returns [List<ExecParam> res]
    :   ep1=execSpParam { $res = new List<ExecParam> { $ep1.res }; }
        (COMMA ep2=execSpParam { $res.Add($ep2.res); })*
    ;

execSpParam returns [ExecSpParam res]
    :   (variable EQUAL)? execSqlParam { $res = new ExecSpParam($variable.res, $execSqlParam.res); }
    ;

execSqlParams returns [List<ExecParam> res]
    :   ep1=execSqlParam { $res = new List<ExecParam> { $ep1.res }; }
        (COMMA ep2=execSqlParam { $res.Add($ep2.res); })*
    ;

execSqlParam returns [ExecSqlParam res]
    :   constant { $res = new ExecSqlParam($constant.res, false); }
    |   variable outputToken? { $res = new ExecSqlParam($variable.res, $outputToken.res); }
    |   DEFAULT { $res = new ExecSqlParam(new DefaultExpression(), false); }
    ;

outputToken returns [bool res]
    :   OUT { $res = true; }
    |   OUTPUT { $res = true; }
    ;

withExecuteOptions returns [List<ExecOption> res]
    :   WITH
        eo1=executeOption { $res = new List<ExecOption> { $eo1.res }; }
        (COMMA eo2=executeOption { $res.Add($eo2.res); })*
    ;

executeOption returns [ExecOption res]
    :   RECOMPILE { $res = new RecompileExecOption(); }
    |   RESULT SETS simpleResultSetsType { $res = new SimpleResultSetsExecOption($simpleResultSetsType.res); }
    |   RESULT SETS LPAREN resultSetsDefinitions RPAREN { $res = new ComplexResultSetsExecOption($resultSetsDefinitions.res); }
    ;

simpleResultSetsType returns [SimpleResultSetsType res]
    :   UNDEFINED { $res = SimpleResultSetsType.Undefined; }
    |   NONE { $res = SimpleResultSetsType.None; }
    ;

resultSetsDefinitions returns [List<ResultSetsDefinition> res]
    :   rsd1=resultSetsDefinition { $res = new List<ResultSetsDefinition> { $rsd1.res }; }
        (COMMA rsd2=resultSetsDefinition { $res.Add($rsd2.res); })*
    ;

resultSetsDefinition returns [ResultSetsDefinition res]
    :   LPAREN resultSetsColumns RPAREN { $res = new ColumnsResultSetsDefinition($resultSetsColumns.res); }
    |   AS OBJECT dbObject { $res = new ObjectResultSetsDefinition($dbObject.res); }
    |   AS TYPE dataType { $res = new TypeResultSetsDefinition($dataType.res); }
    |   AS FOR XML { $res = new ForXmlResultSetsDefinition(); }
    ;

resultSetsColumns returns [List<ResultSetsColumn> res]
    :   rsc1=resultSetsColumn { $res = new List<ResultSetsColumn> { $rsc1.res }; }
        (COMMA rsc2=resultSetsColumn { $res.Add($rsc2.res); })*
    ;

resultSetsColumn returns [ResultSetsColumn res]
    :   identifier dataType collation? nullOrNotNull?
        {
            $res = new ResultSetsColumn($identifier.res, $dataType.res, $collation.res, $nullOrNotNull.res);
        }
    ;

contextClause returns [ContextClause res]
    :   AS LOGIN EQUAL ASCIIStringLiteral { $res = new ContextClause(ContextType.LOGIN, $ASCIIStringLiteral.Text); }
    |   AS USER EQUAL ASCIIStringLiteral { $res = new ContextClause(ContextType.USER, $ASCIIStringLiteral.Text); }
    ;

linkedServer returns [Identifier res]
    :   AT identifier { $res = $identifier.res; }
    ;

// CREATE TABLE

createTableStatement returns [CreateTableStatement res]
    :   CREATE TABLE dbObject (AS FILETABLE)? createTableDefinitions? onPartitionOrFileGroup? textImageOnPartitionOrFileGroup?
        fileStreamOnPartitionOrFileGroup? createTableWithClause?
        {
            $res = new CreateTableStatement($dbObject.res, $FILETABLE != null, $createTableDefinitions.res,
                $onPartitionOrFileGroup.res, $textImageOnPartitionOrFileGroup.res, $fileStreamOnPartitionOrFileGroup.res,
                $createTableWithClause.res);
        }
    ;

createTableDefinitions returns [List<CreateTableDefinition> res]
    :   LPAREN
        ctc1=createTableDefinition { $res = new List<CreateTableDefinition> { $ctc1.res }; }
        (COMMA ctc2=createTableDefinition { $res.Add($ctc2.res); })*
		//after last item TSQL accepts comma without any warnings or error - MSSQL bug?
		COMMA?
        RPAREN
    ;

createTableDefinition returns [CreateTableDefinition res]
        // Note: column_set_name XML COLUMN_SET FOR ALL_SPARSE_COLUMNS not supported.
    :   columnDefinition { $res = $columnDefinition.res; }
    |   computedColumnDefinition { $res = $computedColumnDefinition.res; }
    |   tableConstraint { $res = $tableConstraint.res; }
    ;

columnDefinition returns [ColumnDefinition res]
        // TIMESTAMP data type does not require an identifier, in that case "timestamp" is used as column name.
    :   (TIMESTAMP) => TIMESTAMP columnDefinitionModifiers
        {
            $res = new ColumnDefinition(new Identifier(IdentifierType.Plain, "timestamp"),
                new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.TimeStamp), $columnDefinitionModifiers.res);
        }
    |   identifier dataType columnDefinitionModifiers
        {
            $res = new ColumnDefinition($identifier.res, $dataType.res, $columnDefinitionModifiers.res);
        }
    ;

columnDefinitionModifiers returns [List<ColumnDefinitionModifier> res]
        // The modifiers cannot be in completely arbitrary order. For example collation conflicts with DEFAULT column constraint.
    :   {
            $res = new List<ColumnDefinitionModifier>();
        }
        (
                FILESTREAM
                {
                    $res.Add(new SimpleColumnDefinitionModifier(SimpleColumnDefinitionModifierType.FileStream));
                }
        )?
        (
                collation
                {
                    $res.Add(new CollationColumnDefinitionModifier($collation.res));
                }
        )?
        (
                SPARSE
                {
                    $res.Add(new SimpleColumnDefinitionModifier(SimpleColumnDefinitionModifierType.Sparse));
                }
        )?
        (
                columnDefinitionModifier
                {
                    $res.Add($columnDefinitionModifier.res);
                }
        )*
        {
            if ($res.Count == 0)
            {
                $res = null;
            }
        }
    ;

columnDefinitionModifier returns [ColumnDefinitionModifier res]
    :   NULL { $res = new SimpleColumnDefinitionModifier(SimpleColumnDefinitionModifierType.Null); }
    |   NOT NULL { $res = new SimpleColumnDefinitionModifier(SimpleColumnDefinitionModifierType.NotNull); }
    |   IDENTITY (LPAREN dl1=decimalLiteral COMMA dl2=decimalLiteral RPAREN)? (NOT FOR REPLICATION)?
        {
            $res = new IdentityColumnDefinitionModifier($dl1.res, $dl2.res, $REPLICATION != null);
        }
    |   ROWGUIDCOL { $res = new SimpleColumnDefinitionModifier(SimpleColumnDefinitionModifierType.RowGuidCol); }
    |   columnConstraint { $res = new ConstraintColumnDefinitionModifier($columnConstraint.res); }
    ;

computedColumnDefinition returns [ComputedColumnDefinition res]
    :   identifier AS expression computedColumnPersistenceType? columnConstraint?
        {
            $res = new ComputedColumnDefinition($identifier.res, $expression.res, $computedColumnPersistenceType.res, $columnConstraint.res);
        }
    ;

computedColumnPersistenceType returns [ComputedColumnPersistenceType res]
    :   PERSISTED (NOT NULL)? { $res = ($NOT != null ? ComputedColumnPersistenceType.PersistedNotNull : ComputedColumnPersistenceType.Persisted); }
    ;

onPartitionOrFileGroup returns [PartitionOrFileGroup res]
    :   ON partitionOrFileGroup { $res = $partitionOrFileGroup.res; }
    ;

textImageOnPartitionOrFileGroup returns [PartitionOrFileGroup res]
    :   TEXTIMAGE_ON partitionOrFileGroup { $res = $partitionOrFileGroup.res; }
    ;

fileStreamOnPartitionOrFileGroup returns [PartitionOrFileGroup res]
    :   FILESTREAM_ON partitionOrFileGroup { $res = $partitionOrFileGroup.res; }
    ;

createTableWithClause returns [List<CreateTableOption> res]
    :   WITH LPAREN
        cto1=createTableOption { $res = new List<CreateTableOption> { $cto1.res }; }
        (COMMA cto2=createTableOption { $res.Add($cto2.res); })*
        RPAREN
    ;

createTableOption returns [CreateTableOption res]
    :   dataCompressionClause
        {
            $res = new DataCompressionCreateTableOption($dataCompressionClause.res);
        }
    |   FILETABLE_DIRECTORY EQUAL stringLiteral
        {
            $res = new StringCreateTableOption(StringCreateTableOptionType.FiletableDirectory, $stringLiteral.res);
        }
    |   FILETABLE_COLLATE_FILENAME EQUAL (stringLiteral | DATABASE_DEFAULT)
        {
            $res = new StringCreateTableOption(StringCreateTableOptionType.FiletableCollateFilename, $stringLiteral.res);
        }
    |   FILETABLE_PRIMARY_KEY_CONSTRAINT_NAME EQUAL stringLiteral
        {
            $res = new StringCreateTableOption(StringCreateTableOptionType.FiletablePrimaryKeyConstraintName, $stringLiteral.res);
        }
    |   FILETABLE_STREAMID_UNIQUE_CONSTRAINT_NAME EQUAL stringLiteral
        {
            $res = new StringCreateTableOption(StringCreateTableOptionType.FiletableStreamidUniqueConstraintName, $stringLiteral.res);
        }
    |   FILETABLE_FULLPATH_UNIQUE_CONSTRAINT_NAME EQUAL stringLiteral
        {
            $res = new StringCreateTableOption(StringCreateTableOptionType.FiletableFullpathUniqueConstraintName, $stringLiteral.res);
        }
    ;

// Column constraint

columnConstraint returns [ColumnConstraint res]
    :   (CONSTRAINT identifier)?
        (
                primaryKeyColumnConstraint { $res = $primaryKeyColumnConstraint.res; $res.Name = $identifier.res; }
            |   foreignKeyColumnConstraint { $res = $foreignKeyColumnConstraint.res; $res.Name = $identifier.res; }
            |   checkColumnConstraint { $res = $checkColumnConstraint.res; $res.Name = $identifier.res; }
            |   defaultColumnConstraint { $res = $defaultColumnConstraint.res; $res.Name = $identifier.res; }
        )
    ;

primaryKeyColumnConstraint returns [PrimaryKeyColumnConstraint res]
    :   primaryKeyUniqueClause constraintIndexOptions? onPartitionOrFileGroup?
        {
            $res = new PrimaryKeyColumnConstraint($primaryKeyUniqueClause.res, $constraintIndexOptions.res, $onPartitionOrFileGroup.res);
        }
    ;

foreignKeyColumnConstraint returns [ForeignKeyColumnConstraint res]
    :   (FOREIGN KEY)? REFERENCES dbObject (LPAREN identifier RPAREN)?
        onDeleteAction? onUpdateAction? (NOT FOR REPLICATION)?
        {
            $res = new ForeignKeyColumnConstraint($dbObject.res, $identifier.res, $onDeleteAction.res, $onUpdateAction.res, $REPLICATION != null);
        }
    ;

checkColumnConstraint returns [CheckColumnConstraint res]
    :   CHECK (NOT FOR REPLICATION)? LPAREN expression RPAREN
        {
            $res = new CheckColumnConstraint($expression.res, $REPLICATION != null);
        }
    ;

defaultColumnConstraint returns [DefaultColumnConstraint res]
    :   DEFAULT expression
        {
            $res = new DefaultColumnConstraint($expression.res);
        }
    ;

// Table constraint

tableConstraint returns [TableConstraint res]
    :   (CONSTRAINT identifier)?
        (
                primaryKeyTableConstraint { $res = $primaryKeyTableConstraint.res; $res.Name = $identifier.res; }
            |   foreignKeyTableConstraint { $res = $foreignKeyTableConstraint.res; $res.Name = $identifier.res; }
            |   checkTableConstraint { $res = $checkTableConstraint.res; $res.Name = $identifier.res; }
            |   defaultTableConstraint { $res = $defaultTableConstraint.res; $res.Name = $identifier.res; }
        )
    ;

primaryKeyTableConstraint returns [PrimaryKeyTableConstraint res]
    :   primaryKeyUniqueClause orderedColumnsInParens constraintIndexOptions? onPartitionOrFileGroup?
        {
            $res = new PrimaryKeyTableConstraint($primaryKeyUniqueClause.res, $orderedColumnsInParens.res,
                $constraintIndexOptions.res, $onPartitionOrFileGroup.res);
        }
    ;

foreignKeyTableConstraint returns [ForeignKeyTableConstraint res]
    :   FOREIGN KEY il1=identifierListInParens REFERENCES dbObject 
        il2=identifierListInParens? onDeleteAction? onUpdateAction? (NOT FOR REPLICATION)?
        {
            $res = new ForeignKeyTableConstraint($il1.res, $dbObject.res, $il2.res, $onDeleteAction.res, $onUpdateAction.res, $REPLICATION != null);
        }
    ;

checkTableConstraint returns [CheckTableConstraint res]
    :   CHECK (NOT FOR REPLICATION)? LPAREN expression RPAREN
        {
            $res = new CheckTableConstraint($expression.res, $REPLICATION != null);
        }
    ;

defaultTableConstraint returns [DefaultTableConstraint res]
    :   DEFAULT expression FOR defaultTableConstraintColumns (WITH VALUES)?
        {
            $res = new DefaultTableConstraint($expression.res, $defaultTableConstraintColumns.res, $VALUES != null);
        }
    ;

defaultTableConstraintColumns returns [List<Identifier> res]
    :   identifier { $res = new List<Identifier> { $identifier.res }; }
    |   identifierListInParens { $res = $identifierListInParens.res; }
    ;

// Common rules for column and table constraint

primaryKeyUniqueClause returns [PrimaryKeyUniqueClause res]
    :   primaryKeyOrUnique clusteredOrNonclustered?
        {
            $res = new PrimaryKeyUniqueClause($primaryKeyOrUnique.res, $clusteredOrNonclustered.res ?? ($primaryKeyOrUnique.res == PrimaryKeyOrUnique.PrimaryKey));
        }
    ;

primaryKeyOrUnique returns [PrimaryKeyOrUnique res]
    :   PRIMARY KEY { $res = PrimaryKeyOrUnique.PrimaryKey; }
    |   UNIQUE { $res = PrimaryKeyOrUnique.Unique; }
    ;

clusteredOrNonclustered returns [bool? res]
    :   CLUSTERED { $res = true; }
    |   NONCLUSTERED { $res = false; }
    ;

constraintIndexOptions returns [List<IndexOption> res]
        // We cannot just support all old index options, because old options will cause various
        // conflicts with other parts of the grammar.
    :   withNewIndexOptions { $res = $withNewIndexOptions.res; }
    |   WITH oldFillFactorIndexOption  { $res = new List<IndexOption> { $oldFillFactorIndexOption.res }; }
    ;

onDeleteAction returns [ForeignKeyAction res]
    :   ON DELETE foreignKeyAction { $res = $foreignKeyAction.res; }
    ;

onUpdateAction returns [ForeignKeyAction res]
    :   ON UPDATE foreignKeyAction { $res = $foreignKeyAction.res; }
    ;

foreignKeyAction returns [ForeignKeyAction res]
    :   NO ACTION { $res = ForeignKeyAction.NoAction; }
    |   CASCADE { $res = ForeignKeyAction.Cascade; }
    |   SET NULL { $res = ForeignKeyAction.SetNull; }
    |   SET DEFAULT { $res = ForeignKeyAction.SetDefault; }
    ;

dataCompressionClause returns [DataCompressionClause res]
    :   DATA_COMPRESSION EQUAL dataCompressionType dataCompressionPartitions?
        {
            $res = new DataCompressionClause($dataCompressionType.res, $dataCompressionPartitions.res);
        }
    ;

dataCompressionType returns [DataCompressionType res]
    :   NONE { $res = DataCompressionType.None; }
    |   ROW { $res = DataCompressionType.Row; }
    |   PAGE { $res = DataCompressionType.Page; }
    ;

dataCompressionPartitions returns [List<DataCompressionPartition> res]
    :   ON PARTITIONS LPAREN
        dcp1=dataCompressionPartition { $res = new List<DataCompressionPartition> { $dcp1.res }; }
        (COMMA dcp2=dataCompressionPartition { $res.Add($dcp2.res); })*
        RPAREN
    ;

dataCompressionPartition returns [DataCompressionPartition res]
    :   i1=Integer { $res = new DataCompressionPartition(Int32.Parse($i1.Text), Int32.Parse($i1.Text)); }
    |   i2=Integer TO i3=Integer { $res = new DataCompressionPartition(Int32.Parse($i2.Text), Int32.Parse($i3.Text)); }
    ;

// ALTER TABLE

alterTableStatement returns [AlterTableStatement res]
    :   ALTER TABLE simpleTableSource alterTableAction { $res = new AlterTableStatement($simpleTableSource.res, $alterTableAction.res); }
    ;

alterTableAction returns [AlterTableAction res]
    :   alterColumnAlterTableAction { $res = $alterColumnAlterTableAction.res; }
    |   addAlterTableAction { $res = $addAlterTableAction.res; }
    |   dropAlterTableAction { $res = $dropAlterTableAction.res; }
    |   checkAlterTableAction { $res = $checkAlterTableAction.res; }
    |   triggerAlterTableAction { $res = $triggerAlterTableAction.res; }
    |   changeTrackingAlterTableAction { $res = $changeTrackingAlterTableAction.res; }
    |   switchPartitionAlterTableAction { $res = $switchPartitionAlterTableAction.res; }
    |   setFilestreamAlterTableAction { $res = $setFilestreamAlterTableAction.res; }
    |   rebuildPartitionAlterTableAction { $res = $rebuildPartitionAlterTableAction.res; }
    |   tableOptionAlterTableAction { $res = $tableOptionAlterTableAction.res; }
    ;

alterColumnAlterTableAction returns [AlterTableAction res]
        // nullOrNotNull must be three state, Default, Null, NotNull
    :   ALTER COLUMN identifier dataType collation? nullOrNotNull? SPARSE?
        {
            $res = new AlterColumnDefineAlterTableAction($identifier.res, $dataType.res,
                $collation.res, $nullOrNotNull.res, $SPARSE != null);
        }
    |   ALTER COLUMN identifier addOrDrop alterColumnModifier
        {
            $res = new AlterColumnAddDropAlterTableAction(
                $identifier.res, $addOrDrop.res, $alterColumnModifier.res);
        }
    ;

nullOrNotNull returns [bool? res]
    :   NULL { $res = true; }
    |   NOT NULL { $res = false; }
    ;

addOrDrop returns [AddOrDrop res]
    :   ADD { $res = AddOrDrop.Add; }
    |   DROP { $res = AddOrDrop.Drop; }
    ;

alterColumnModifier returns [AlterColumnModifier res]
    :   ROWGUIDCOL { $res = AlterColumnModifier.RowGuidCol; }
    |   PERSISTED { $res = AlterColumnModifier.Persisted; }
    |   NOT FOR REPLICATION { $res = AlterColumnModifier.NotForReplication; }
    |   SPARSE { $res = AlterColumnModifier.Sparse; }
    ;

addAlterTableAction returns [AddAlterTableAction res]
    :   withCheckNoCheck? ADD addAlterTableDefinitions
        {
            $res = new AddAlterTableAction($withCheckNoCheck.res, $addAlterTableDefinitions.res);
        }
    ;

addAlterTableDefinitions returns [List<CreateTableDefinition> res]
    :   ctd1=createTableDefinition { $res = new List<CreateTableDefinition> { $ctd1.res }; }
        (COMMA ctd2=createTableDefinition { $res.Add($ctd2.res); })*
    ;

withCheckNoCheck returns [bool? res]
        // Default: WITH CHECK for new constraints, WITH NOCHECK for re-enabled constraints -> need 3 states
    :   WITH CHECK { $res = true; }
    |   WITH NOCHECK { $res = false; }
    ;

dropAlterTableAction returns [DropAlterTableAction res]
    :   DROP dropAlterTableDefinitions { $res = new DropAlterTableAction($dropAlterTableDefinitions.res); }
    ;

dropAlterTableDefinitions returns [List<DropAlterTableDefinition> res]
    :   datd1=dropAlterTableDefinition { $res = new List<DropAlterTableDefinition> { $datd1.res }; }
        (COMMA datd2=dropAlterTableDefinition { $res.Add($datd2.res); })*
    ;

dropAlterTableDefinition returns [DropAlterTableDefinition res]
    :   CONSTRAINT? identifier dropClusteredConstraintOptions?
        {
            $res = new DropConstraintAlterTableDefinition($identifier.res, $dropClusteredConstraintOptions.res);
        }
    |   COLUMN identifier
        {
            $res = new DropColumnAlterTableDefinition($identifier.res);
        }
    ;

dropClusteredConstraintOptions returns [List<DropClusteredConstraintOption> res]
    :   WITH LPAREN
        dcco1=dropClusteredConstraintOption { $res = new List<DropClusteredConstraintOption> { $dcco1.res }; }
        (COMMA dcco2=dropClusteredConstraintOption { $res.Add($dcco2.res); })*
        RPAREN
    ;

dropClusteredConstraintOption returns [DropClusteredConstraintOption res]
    :   MAXDOP EQUAL Integer { $res = new MaxDopDropClusteredConstraintOption(Int32.Parse($Integer.Text)); }
    |   ONLINE EQUAL onOff  { $res = new OnlineDropClusteredConstraintOption($onOff.res ?? false); }
    |   MOVE TO partitionOrFileGroup { $res = new MoveToDropClusteredConstraintOption($partitionOrFileGroup.res); }
    ;

checkAlterTableAction returns [CheckAlterTableAction res]
    :   withCheckNoCheck? checkNoCheck CONSTRAINT allOrIdentifierList
        {
            $res = new CheckAlterTableAction($withCheckNoCheck.res, $checkNoCheck.res, $allOrIdentifierList.res);
        }
    ;

checkNoCheck returns [bool res]
    :   CHECK { $res = true; }
    |   NOCHECK { $res = false; }
    ;

allOrIdentifierList returns [List<Identifier> res]
    :   ALL { $res = new List<Identifier>(); }
    |   i1=identifier { $res = new List<Identifier> { $i1.res }; }
        (COMMA i2=identifier { $res.Add($i2.res); })*
    ;

triggerAlterTableAction returns [TriggerAlterTableAction res]
    :   enableDisable TRIGGER allOrIdentifierList
        {
            $res = new TriggerAlterTableAction($enableDisable.res ?? false, $allOrIdentifierList.res);
        }
    ;

changeTrackingAlterTableAction returns [ChangeTrackingAlterTableAction res]
        // default for TRACK_COLUMNS_UPDATED is off
    :   enableDisable CHANGE_TRACKING (WITH LPAREN TRACK_COLUMNS_UPDATED EQUAL onOff RPAREN)?
        {
            $res = new ChangeTrackingAlterTableAction($enableDisable.res ?? false, $onOff.res ?? false);
        }
    ;

enableDisable returns [bool? res]
    :   ENABLE { $res = true; }
    |   DISABLE { $res = false; }
    ;

switchPartitionAlterTableAction returns [SwitchPartitionAlterTableAction res]
    :   SWITCH (PARTITION e1=expression)? TO dbObject (PARTITION e2=expression)?
        {
            $res = new SwitchPartitionAlterTableAction($e1.res, $dbObject.res, $e2.res);
        }
    ;

setFilestreamAlterTableAction returns [SetFilestreamAlterTableAction res]
    :   SET LPAREN FILESTREAM_ON EQUAL partitionOrFileGroup RPAREN
        {
            $res = new SetFilestreamAlterTableAction($partitionOrFileGroup.res);
        }
    ;

partitionOrFileGroup returns [PartitionOrFileGroup res]
    :   i1=identifier LPAREN i2=identifier RPAREN { $res = new Partition($i1.res, $i2.res); }
    |   i3=identifier { $res = new FileGroup($i3.res); }
    ;

rebuildPartitionAlterTableAction returns [RebuildPartitionAlterTableAction res]
    :   REBUILD rebuildPartitionClause? withNewIndexOptions?
        {
            $res = new RebuildPartitionAlterTableAction($rebuildPartitionClause.res, $withNewIndexOptions.res);
        }
    ;

rebuildPartitionClause returns [RebuildPartitionClause res]
        // default is ALL
    :   PARTITION EQUAL
        (
                ALL { $res = null; }
            |   Integer { $res = new RebuildPartitionClause(Int32.Parse($Integer.Text)); }
        )
    ;

tableOptionAlterTableAction returns [AlterTableAction res]
    :   SET LPAREN LOCK_ESCALATION EQUAL lockEscalation RPAREN
        {
            $res = new LockEscalationTableOptionAlterTableAction($lockEscalation.res);
        }
    |   enableDisable FILETABLE_NAMESPACE (SET LPAREN FILETABLE_DIRECTORY EQUAL stringLiteral RPAREN)?
        {
            $res = new FiletableTableOptionAlterTableAction($enableDisable.res, $stringLiteral.res);
        }
    |   SET LPAREN FILETABLE_DIRECTORY EQUAL stringLiteral RPAREN
        {
            $res = new FiletableTableOptionAlterTableAction(null, $stringLiteral.res);
        }
    ;

lockEscalation returns [LockEscalationType res]
    :   AUTO { $res = LockEscalationType.Auto; }
    |   TABLE { $res = LockEscalationType.Table; }
    |   DISABLE { $res = LockEscalationType.Disable; }
    ;

// DROP TABLE

simpleTableSourceList returns [List<DbObjectTableSource> res]
    :   sts1=simpleTableSource { $res = new List<DbObjectTableSource> { $sts1.res }; }
        (COMMA sts2=simpleTableSource { $res.Add ($sts2.res); })*
    ;

simpleTableSource returns [DbObjectTableSource res]
    :   dbObject { $res = new DbObjectTableSource($dbObject.res, null, null, null); }
	;

dropTableStatement returns [DropTableStatement res]
    :   DROP TABLE simpleTableSourceList { $res = new DropTableStatement($simpleTableSourceList.res); }
    ;

// CREATE INDEX

createIndexStatement returns [CreateIndexStatement res]
    :   newCreateIndexStatement { $res = $newCreateIndexStatement.res; }
    |   oldCreateIndexStatement { $res = $oldCreateIndexStatement.res; }
    ;

newCreateIndexStatement returns [CreateIndexStatement res]
    :   CREATE UNIQUE? clusteredOrNonclustered? INDEX identifier ON indexTarget orderedColumnsInParens (INCLUDE identifierListInParens)?
        (WHERE expression)? withNewIndexOptions? onPartitionOrFileGroup? fileStreamOnPartitionOrFileGroup?
        {
            $res = new CreateIndexStatement($UNIQUE != null, $clusteredOrNonclustered.res, $identifier.res,
                $indexTarget.res, $orderedColumnsInParens.res, $identifierListInParens.res, $expression.res, $withNewIndexOptions.res,
                $onPartitionOrFileGroup.res, $fileStreamOnPartitionOrFileGroup.res);
        }
    ;

oldCreateIndexStatement returns [CreateIndexStatement res]
        // This has to be a separate rule because of conflicts between withOldIndexOptions and fileStreamOnPartitionOrFileGroup.
        // Also note that if there are no WITH options, we consider it a new style statement.
    :   CREATE UNIQUE? clusteredOrNonclustered? INDEX identifier ON indexTarget orderedColumnsInParens withOldIndexOptions onPartitionOrFileGroup?
        {
            $res = new CreateIndexStatement($UNIQUE != null, $clusteredOrNonclustered.res, $identifier.res,
                $indexTarget.res, $orderedColumnsInParens.res, null, null, $withOldIndexOptions.res, $onPartitionOrFileGroup.res, null);
        }
    ;

indexTarget returns [DbObjectIndexTarget res]
    :   dbObject { $res = new DbObjectIndexTarget($dbObject.res); }
    ;

withNewIndexOptions returns [List<IndexOption> res]
    :   WITH newIndexOptions { $res = $newIndexOptions.res; }
    ;

newIndexOptions returns [List<IndexOption> res]
    :   LPAREN
        nio1=newIndexOption { $res = new List<IndexOption> { $nio1.res }; }
        (COMMA nio2=newIndexOption { $res.Add($nio2.res); })*
        RPAREN
    ;

newIndexOption returns [IndexOption res]
    :   PAD_INDEX EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.PadIndex, $onOff.res ?? false); }
    |   FILLFACTOR EQUAL Integer { $res = new FillFactorIndexOption(Int32.Parse($Integer.Text)); }
    |   IGNORE_DUP_KEY EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.IgnoreDupKey, $onOff.res ?? false); }
    |   STATISTICS_NORECOMPUTE EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.StatisticsNorecompute, $onOff.res ?? false); }
    |   DROP_EXISTING EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.DropExisting, $onOff.res ?? false); }
    |   ALLOW_ROW_LOCKS EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.AllowRowLocks, $onOff.res ?? false); }
    |   ALLOW_PAGE_LOCKS EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.AllowPageLocks, $onOff.res ?? false); }
    |   SORT_IN_TEMPDB EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.SortInTempDB, $onOff.res ?? false); }
    |   ONLINE EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.Online, $onOff.res ?? false); }
    |   MAXDOP EQUAL Integer { $res = new MaxDopIndexOption(Int32.Parse($Integer.Text)); }
    |   dataCompressionClause { $res = new DataCompressionIndexOption($dataCompressionClause.res); }
    ;

onOff returns [bool? res]
    :   ON { $res = true; }
    |   OFF { $res = false; }
    ;

withOldIndexOptions returns [List<IndexOption> res]
    :   WITH
        oio1=oldIndexOption { $res = new List<IndexOption> { $oio1.res }; }
        (COMMA oio2=oldIndexOption { $res.Add($oio2.res); })*
    ;

oldIndexOption returns [IndexOption res]
    :   oldFillFactorIndexOption { $res = $oldFillFactorIndexOption.res; }
    |   PAD_INDEX { $res = new SimpleIndexOption(SimpleIndexOptionType.PadIndex, true); }
    |   SORT_IN_TEMPDB { $res = new SimpleIndexOption(SimpleIndexOptionType.SortInTempDB, true); }
    |   IGNORE_DUP_KEY { $res = new SimpleIndexOption(SimpleIndexOptionType.IgnoreDupKey, true); }
    |   STATISTICS_NORECOMPUTE { $res = new SimpleIndexOption(SimpleIndexOptionType.StatisticsNorecompute, true); }
    |   DROP_EXISTING { $res = new SimpleIndexOption(SimpleIndexOptionType.DropExisting, true); }
    ;

oldFillFactorIndexOption returns [IndexOption res]
        // Some rules support only FILLFACTOR and they cannot include the rest of oldIndexOption because of grammar conflicts.
    :   FILLFACTOR EQUAL Integer { $res = new FillFactorIndexOption(Int32.Parse($Integer.Text)); }
    ;

// ALTER INDEX

alterIndexStatement returns [AlterIndexStatement res]
    :   ALTER INDEX identifierOrAll ON simpleTableSource alterIndexAction
        {
            $res = new AlterIndexStatement($identifierOrAll.res, $simpleTableSource.res, $alterIndexAction.res);
        }
    ;

identifierOrAll returns [Identifier res]
    :   ALL { $res = new Identifier(IdentifierType.Plain, ""); }
    |   identifier { $res = $identifier.res; }
    ;

alterIndexAction returns [AlterIndexAction res]
    :   rebuildAlterIndexAction { $res = $rebuildAlterIndexAction.res; }
    |   disableAlterIndexAction { $res = $disableAlterIndexAction.res; }
    |   reorganizeAlterIndexAction { $res = $reorganizeAlterIndexAction.res; }
    |   setAlterIndexAction { $res = $setAlterIndexAction.res; }
    ;

rebuildAlterIndexAction returns [RebuildAlterIndexAction res]
    :   REBUILD rebuildPartitionClause? withNewIndexOptions?
        {
            $res = new RebuildAlterIndexAction($rebuildPartitionClause.res, $withNewIndexOptions.res);
        }
    ;

disableAlterIndexAction returns [DisableAlterIndexAction res]
    :   DISABLE { $res = new DisableAlterIndexAction(); }
    ;

reorganizeAlterIndexAction returns [ReorganizeAlterIndexAction res]
    :   REORGANIZE (PARTITION EQUAL Integer)? (WITH LPAREN LOB_COMPACTION EQUAL onOff RPAREN)?
        {
            $res = new ReorganizeAlterIndexAction($Integer != null ? Int32.Parse($Integer.Text) : -1, $onOff.res ?? true);
        }
    ;

setAlterIndexAction returns [SetAlterIndexAction res]
    :   SET newIndexOptions { $res = new SetAlterIndexAction($newIndexOptions.res); }
    ;

// DROP INDEX

dropIndexStatement returns [DropIndexStatement res]
    :   DROP INDEX
        (
                newDropIndexActions { $res = new DropIndexStatement($newDropIndexActions.res); }
            |   oldDropIndexActions { $res = new DropIndexStatement($oldDropIndexActions.res); }
        )
    ;

newDropIndexActions returns [List<DropIndexAction> res]
    :   ndia1=newDropIndexAction { $res = new List<DropIndexAction> { $ndia1.res }; }
        (COMMA ndia2=newDropIndexAction { $res.Add($ndia2.res); })*
    ;

newDropIndexAction returns [DropIndexAction res]
    :   identifier ON simpleTableSource withDropClusteredIndexOptions?
        {
            $res = new DropIndexAction($identifier.res, $simpleTableSource.res, $withDropClusteredIndexOptions.res);
        }
    ;

withDropClusteredIndexOptions returns [List<IndexOption> res]
    :   WITH LPAREN
        dcio1=dropClusteredIndexOption { $res = new List<IndexOption> { $dcio1.res }; }
        (COMMA dcio2=dropClusteredIndexOption { $res.Add($dcio2.res); })*
        RPAREN
    ;

dropClusteredIndexOption returns [IndexOption res]
    :   ONLINE EQUAL onOff { $res = new SimpleIndexOption(SimpleIndexOptionType.Online, $onOff.res ?? false); }
    |   MAXDOP EQUAL Integer { $res = new MaxDopIndexOption(Int32.Parse($Integer.Text)); }
    |   MOVE TO partitionOrFileGroup { $res = new PartitionOrFileGroupIndexOption(PartitionOrFileGroupIndexOptionType.MoveTo, $partitionOrFileGroup.res); }
    |   FILESTREAM_ON partitionOrFileGroup { $res = new PartitionOrFileGroupIndexOption(PartitionOrFileGroupIndexOptionType.FileStreamOn, $partitionOrFileGroup.res); }
    ;

oldDropIndexActions returns [List<DropIndexAction> res]
    :   odia1=oldDropIndexAction { $res = new List<DropIndexAction> { $odia1.res }; }
        (COMMA odia2=oldDropIndexAction { $res.Add($odia2.res); })*
    ;

oldDropIndexAction returns [DropIndexAction res]
        // Cannot use "dbObject DOT identifier", because it causes a conflict between end of dbObject and DOT identifier.
        @init { List<Identifier> identifiers = new List<Identifier>(); }
    :   i1=identifier { identifiers.Add($i1.res); }
        (DOT (DOT { identifiers.Add(new Identifier(IdentifierType.Plain, "")); })* i2=identifier { identifiers.Add($i2.res); })*
        DOT i3=identifier
        { $res = new DropIndexAction($i3.res, new DbObjectTableSource(new DbObject(identifiers), null, null, null)); }
    ;

// CREATE VIEW

createViewStatement returns [CreateViewStatement res]
    :   CREATE VIEW dbObject identifierListInParens? withViewAttributes? AS selectStatement (WITH CHECK OPTION)?
        {
            $res = new CreateViewStatement($dbObject.res, $identifierListInParens.res, $withViewAttributes.res, $selectStatement.res, $CHECK != null);
        }
    ;

withViewAttributes returns [List<ViewAttribute> res]
    :   WITH
        va1=viewAttribute { $res = new List<ViewAttribute> { $va1.res }; }
        (COMMA va2=viewAttribute { $res.Add($va2.res); })*
    ;

viewAttribute returns [ViewAttribute res]
    :   ENCRYPTION { $res = ViewAttribute.Encryption; }
    |   SCHEMABINDING { $res = ViewAttribute.SchemaBinding; }
    |   VIEW_METADATA { $res = ViewAttribute.ViewMetadata; }
    ;

// ALTER VIEW

alterViewStatement returns [AlterViewStatement res]
    :   ALTER VIEW dbObject identifierListInParens? withViewAttributes? AS selectStatement (WITH CHECK OPTION)?
        {
            $res = new AlterViewStatement($dbObject.res, $identifierListInParens.res, $withViewAttributes.res, $selectStatement.res, $CHECK != null);
        }
    ;

// DROP VIEW

dropViewStatement returns [DropViewStatement res]
    :   DROP VIEW dbObjectList { $res = new DropViewStatement($dbObjectList.res); }
    ;

// CREATE TRIGGER
//Trigger on an INSERT, UPDATE, or DELETE statement to a table or view (DML Trigger)
//CREATE TRIGGER [ schema_name . ]trigger_name 
//ON { table | view } 
//[ WITH <dml_trigger_option> [ ,...n ] ]
//{ FOR | AFTER | INSTEAD OF } 
//{ [ INSERT ] [ , ] [ UPDATE ] [ , ] [ DELETE ] } 
//[ NOT FOR REPLICATION ] 
//AS { sql_statement  [ ; ] [ ,...n ] | EXTERNAL NAME <method specifier [ ; ] > }
//
//<dml_trigger_option> ::=
//    [ ENCRYPTION ]//
//    [ EXECUTE AS Clause ]
//
//<method_specifier> ::=
//    assembly_name.class_name.method_name
createDmlTriggerStatement returns [CreateDmlTriggerStatement res]
    : CREATE TRIGGER name = dbObject ON table = dbObject 
    	(WITH dmlTriggerOptions)? 
    	triggerApplicationOption 
    	triggerDmlOperations
    	(notForReplication)?
   	AS 
    	triggerAction
    	{
		$res = new CreateDmlTriggerStatement ($name.res, $table.res, $dmlTriggerOptions.res, $triggerApplicationOption.res, 
		$triggerDmlOperations.res, $notForReplication.res ?? false, $triggerAction.res);
    	}
    ;

triggerAction returns [TriggerAction res]
    : EXTERNAL NAME dbObject { $res = new TriggerAction($dbObject.res); }
    | simpleStatements { $res = new TriggerAction(new BlockStatement($simpleStatements.res)); }
    ;

notForReplication returns [bool? res]
    : (NOT FOR REPLICATION) {$res = true; }
    ;

triggerApplicationOption returns [TriggerApplType res]
    : FOR { $res = TriggerApplType.FOR; }
    | AFTER { $res = TriggerApplType.AFTER; }
    | INSTEAD OF { $res = TriggerApplType.INSTEAD_OF; }
    ;

triggerDmlOperations returns [List<TriggerDmlOperationType> res]
    :  do1 = triggerDmlOperation { $res = new List<TriggerDmlOperationType> {$do1.res}; }
     (COMMA do2 = triggerDmlOperation { $res.Add($do2.res); })*
    ;

triggerDmlOperation returns [TriggerDmlOperationType res]
    : INSERT { $res = TriggerDmlOperationType.INSERT; }
    | UPDATE { $res = TriggerDmlOperationType.UPDATE; }
    | DELETE { $res = TriggerDmlOperationType.DELETE; }
    ;

dmlTriggerOptions returns [List<DmlTriggerOption> res]
    : op1 = dmlTriggerOption { $res = new List<DmlTriggerOption> {$op1.res}; }
    | (COMMA op2 = dmlTriggerOption { $res.Add($op2.res); })*
    ;

dmlTriggerOption returns [DmlTriggerOption res]
    : ENCRYPTION { $res = new DmlTriggerOption(true); }
    | EXECUTE AS executeAsContext { $res = new DmlTriggerOption ($executeAsContext.res); }
    ;

// Dml
// DROP TRIGGER [schema_name.]trigger_name [ ,...n ] [ ; ]
dropTriggerStatement returns [DropTriggerStatement res]
    : DROP TRIGGER dbObject { $res = new DropTriggerStatement($dbObject.res); }
    ;

//RAISERROR ( { msg_id | msg_str | @local_variable }
//    { ,severity ,state }
//    [ ,argument [ ,...n ] ] )
//    [ WITH option [ ,...n ] ] -- TO DO
raisErrorStatement returns [RaisErrorStatement res]
    : RAISERROR LPAREN execSqlParams RPAREN { $res = new RaisErrorStatement($execSqlParams.res); }
    ;

/////////////////////////////////////////////////////////////////
//--Transact-SQL Scalar Function Syntax
// CREATE FUNCTION [ schema_name. ] function_name 
//		<functionParams>
// RETURNS return_data_type
//     [ WITH <function_option> [ ,...n ] ]
//     [ AS ] <functionBody>
//
//--Transact-SQL Inline Table-Valued Function Syntax 
// CREATE FUNCTION [ schema_name. ] function_name 
//		<functionParams>
// RETURNS TABLE
//     [ WITH <function_option> [ ,...n ] ]
//     [ AS ] <tableValuedFunctionBody>
//
//--Transact-SQL Multistatement Table-valued Function Syntax
// CREATE FUNCTION [ schema_name. ] function_name 
//		<functionParams>
// RETURNS @return_variable TABLE <table_type_definition>
//     [ WITH <function_option> [ ,...n ] ]
//     [ AS ] <functionBody>
//////////////////////////////////////////////////////////////////
createFunctionStatement returns [CreateFunctionStatement res]
	: CREATE FUNCTION dbObject functionParams RETURNS dataType withFunctionOption? AS? blockStatement SEMICOLON?
		{ $res = new CreateScalarFunctionStatement($dbObject.res, $functionParams.res, $dataType.res, $withFunctionOption.res, $blockStatement.res); }
	| CREATE FUNCTION dbObject functionParams RETURNS TABLE withFunctionOption? AS? RETURN selectStatement SEMICOLON?
		{ $res = new CreateTableValuedFunctionStatement($dbObject.res, $functionParams.res, $withFunctionOption.res, $selectStatement.res); }
	| CREATE FUNCTION dbObject functionParams RETURNS variable TABLE createTableDefinitions withFunctionOption? AS? blockStatement SEMICOLON?
		{ $res = new CreateMultiStatementFunctionStatement($dbObject.res, $functionParams.res, $variable.res, $createTableDefinitions.res, $withFunctionOption.res, $blockStatement.res); }
	;

/////////////////////////////////////////////////////////////////
// <functionParams>::=
// ( [ { @parameter_name [ AS ][ type_schema_name. ] parameter_data_type 
//     [ = default ] [ READONLY ] } 
//     [ ,...n ]
//   ]
// )
/////////////////////////////////////////////////////////////////
functionParams returns [List<ProcedureParameter> res]
	: LPAREN procedureParameterList RPAREN { $res = $procedureParameterList.res; }
	;

/////////////////////////////////////////////////////////////////
// <function_option>::= 
// {
//     [ ENCRYPTION ]
//   | [ SCHEMABINDING ]
//   | [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
//   | [ EXECUTE_AS_Clause ]
// }
/////////////////////////////////////////////////////////////////
withFunctionOption returns [FunctionOption res]
	: WITH functionOption { $res = $functionOption.res; }
	;

functionOption returns [FunctionOption res]
	: ENCRYPTION { $res = new SimpleFunctionOption(FunctionOptionType.ENCRYPTION); }
	| SCHEMABINDING { $res = new SimpleFunctionOption(FunctionOptionType.SCHEMABINDING); }
	| RETURNS NULL ON NULL INPUT { $res = new SimpleFunctionOption(FunctionOptionType.RETURNS_NULL_ON_NULL_INPUT); }
	| CALLED ON NULL INPUT { $res = new SimpleFunctionOption(FunctionOptionType.CALLED_ON_NULL_INPUT); }
	| EXECUTE AS executeAsContext { $res = new ExecuteAsFunctionOption($executeAsContext.res); }
	;

////////////////////////////////////////////////////////////////
// <tableValuedFunctionBody>::=
//     [ ( ] select_stmt [ ) ]
// [ ; ]
////////////////////////////////////////////////////////////////
tableValuedFunctionBody returns [SelectStatement res]
	: selectStatement { $res = $selectStatement.res; }
	;

// CREATE PROCEDURE

// Note: CLR procedures not supported.
createProcedureStatement returns [CreateProcedureStatement res]
    :   CREATE (PROC | PROCEDURE) dbObject (SEMICOLON Integer)? procedureParameters? withProcedureOptions? (FOR REPLICATION)? AS simpleStatements
        {
            $res = new CreateProcedureStatement($dbObject.res, $Integer == null ? -1 : Int32.Parse($Integer.Text), 
                $procedureParameters.res, $withProcedureOptions.res, $REPLICATION != null, $simpleStatements.res);
        }
    ;

procedureParameters returns [List<ProcedureParameter> res]
    :   LPAREN procedureParameterList RPAREN { $res = $procedureParameterList.res; }
    |   procedureParameterList { $res = $procedureParameterList.res; }
    ;

procedureParameterList returns [List<ProcedureParameter> res]
    :   pp1=procedureParameter { $res = new List<ProcedureParameter> { $pp1.res }; }
        (COMMA pp2=procedureParameter { $res.Add($pp2.res); })*
    ;

procedureParameter returns [ProcedureParameter res]
    :   Variable
        (
                AS? dataType (EQUAL constant)? outputToken? READONLY?
                {
                    $res = new DataTypeProcedureParameter($Variable.Text, $dataType.res, $constant.res, $outputToken.res, $READONLY != null);
                }
            |   CURSOR VARYING (OUT | OUTPUT)
                {
                    $res = new CursorProcedureParameter();
                }
        )
    ;

withProcedureOptions returns [List<ProcedureOption> res]
    :   WITH
        po1=procedureOption { $res = new List<ProcedureOption> { $po1.res }; }
        (COMMA po2=procedureOption { $res.Add($po2.res); })*
    ;

procedureOption returns [ProcedureOption res]
    :   ENCRYPTION { $res = new SimpleProcedureOption(SimpleProcedureOptionType.Encryption); }
    |   RECOMPILE { $res = new SimpleProcedureOption(SimpleProcedureOptionType.Recompile); }
    |   EXECUTE AS executeAsContext { $res = new ExecuteAsProcedureOption($executeAsContext.res); }
    ;

executeAsContext returns [ExecuteAsContext res]
    :   CALLER { $res = new SimpleExecuteAsContext(SimpleExecuteAsContextType.Caller); }
    |   SELF { $res = new SimpleExecuteAsContext(SimpleExecuteAsContextType.Self); }
    |   OWNER { $res = new SimpleExecuteAsContext(SimpleExecuteAsContextType.Owner); }
    |   stringLiteral { $res = new StringExecuteAsContext($stringLiteral.res); }
    ;

// ALTER PROCEDURE

// Note: CLR procedures not supported.
alterProcedureStatement returns [AlterProcedureStatement res]
    :   ALTER (PROC | PROCEDURE) dbObject (SEMICOLON Integer)? procedureParameters? withProcedureOptions? (FOR REPLICATION)? AS simpleStatements
        {
            $res = new AlterProcedureStatement($dbObject.res, $Integer == null ? -1 : Int32.Parse($Integer.Text), 
                $procedureParameters.res, $withProcedureOptions.res, $REPLICATION != null, $simpleStatements.res);
        }
    ;

// DROP PROCEDURE

dropProcedureStatement returns [DropProcedureStatement res]
    :   DROP (PROC | PROCEDURE) dbObjectList
        {
            $res = new DropProcedureStatement($dbObjectList.res);
        }
    ;

// Control-of-Flow

ifStatement returns [IfStatement res]
        // Must use ntExpression, because normal expression causes conflicts here.
    :   IF ntExpression ss1=simpleStatement
        (
                // Predicate required to solve the famous dangling ELSE conflict.
                (SEMICOLON* ELSE) => SEMICOLON* ELSE ss2=simpleStatement
        )?
        {
            $res = new IfStatement($ntExpression.res, $ss1.res, $ss2.res);
        }
    ;

whileStatement returns [WhileStatement res]
        // Must use ntExpression, because normal expression causes conflicts here.
    :   WHILE ntExpression simpleStatement { $res = new WhileStatement($ntExpression.res, $simpleStatement.res); }
    ;

breakStatement returns [BreakStatement res]
    :   BREAK { $res = new BreakStatement(); }
    ;

continueStatement returns [ContinueStatement res]
    :   CONTINUE { $res = new ContinueStatement(); }
    ;

gotoStatement returns [GotoStatement res]
    :   GOTO identifier { $res = new GotoStatement($identifier.res); }
    ;

labelStatement returns [LabelStatement res]
        // Since labelStatement starts a new statement, it has to use statementEndIdentifier instead of identifier.
    :   statementEndIdentifier COLON { $res = new LabelStatement($statementEndIdentifier.res); }
    ;

waitForStatement returns [WaitForStatement res]
        // Note: No support for RECEIVE, GET CONVERSATION GROUP or TIMEOUT (applicable only to Service Broker).
    :   WAITFOR delayOrTime variableOrConstant { $res = new WaitForStatement($delayOrTime.res, $variableOrConstant.res); }
    ;

delayOrTime returns [DelayOrTime res]
    :   DELAY { $res = DelayOrTime.Delay; }
    |   TIME { $res = DelayOrTime.Time; }
    ;

tryStatement returns [TryStatement res]
    :   BEGIN TRY ss1=simpleStatements END TRY BEGIN CATCH ss2=simpleStatements? END CATCH
        {
            $res = new TryStatement($ss1.res, $ss2.res);
        }
    ;

throwStatement returns [ThrowStatement res]
    :   THROW (voc1=variableOrConstant COMMA voc2=variableOrConstant COMMA voc3=variableOrConstant)?
        {
            $res = new ThrowStatement($voc1.res, $voc2.res, $voc3.res);
        }
    ;

returnStatement returns [ReturnStatement res]
        // Conflict between expression and next statement. It seems SQL Server prefers expression.
    :   RETURN
        (
                (ntExpression) => ntExpression { $res = new ReturnStatement($ntExpression.res); }
            |   { $res = new ReturnStatement(null); }
        )
    ;

blockStatement returns [BlockStatement res]
    :   BEGIN simpleStatements END { $res = new BlockStatement($simpleStatements.res); }
    ;

// DECLARE, SET

declareStatement returns [DeclareStatement res]
    :   DECLARE variableDeclarations { $res = new DeclareStatement($variableDeclarations.res); }
    ;

variableDeclarations returns [List<VariableDeclaration> res]
    :   vd1=variableDeclaration { $res = new List<VariableDeclaration> { $vd1.res }; }
        (COMMA vd2=variableDeclaration { $res.Add($vd2.res); })*
    ;

variableDeclaration returns [VariableDeclaration res]
    :   variable
        (
                AS? dataType (EQUAL expression)?
                {
                    $res = new ScalarVariableDeclaration($variable.res, $dataType.res, $expression.res);
                }
            |   AS? TABLE LPAREN declareTableDefinitions RPAREN
                {
                    $res = new TableVariableDeclaration($variable.res, $declareTableDefinitions.res);
                }
        )
    ;

cursorDeclaration returns [DeclareStatement res]   
  :   DECLARE cursorSource CURSOR cursorProperties? FOR? selectStatement cursorForUpdate? {$res = new DeclareStatement(new List<VariableDeclaration> {new CursorVariableDeclaration($cursorSource.res, $cursorProperties.res, $selectStatement.res, $cursorForUpdate.res)}) ;}
  ;

cursorForUpdate returns [CursorForUpdate res]
	: FOR UPDATE (OF cursorUpdateColumns)? {$res = new CursorForUpdate($cursorUpdateColumns.res); }
	;

cursorUpdateColumns returns [List<Identifier> res]
	: col1=identifier { $res= new List<Identifier> { $col1.res }; }
	 (COMMA col2=identifier { $res.Add($col2.res); })*
	;

cursorProperties returns [List<CursorProperty> res]
    :   cp1=cursorProperty { $res = new List<CursorProperty> { $cp1.res }; }
        (cp2=cursorProperty { $res.Add($cp2.res); }
        )*
    ;

cursorProperty returns [CursorProperty res]
    :   LOCAL { $res = new CursorProperty (CursorPropertyType.Local); }
    |   GLOBAL { $res = new CursorProperty (CursorPropertyType.Global); }
    |   FORWARD_ONLY { $res = new CursorProperty (CursorPropertyType.ForwardOnly); }
    |   SCROLL { $res = new CursorProperty (CursorPropertyType.Scroll); }
    |   STATIC { $res = new CursorProperty (CursorPropertyType.Static); }
    |   KEYSET { $res = new CursorProperty (CursorPropertyType.Keyset); }
    |   DYNAMIC { $res = new CursorProperty (CursorPropertyType.Dynamic); }
    |   FAST_FORWARD { $res = new CursorProperty (CursorPropertyType.FastForward); }
    |   READ_ONLY { $res = new CursorProperty (CursorPropertyType.ReadOnly); }
    |   SCROLL_LOCKS { $res = new CursorProperty (CursorPropertyType.ScrollLocks); }
    |   OPTIMISTIC { $res = new CursorProperty (CursorPropertyType.Optimistic); }
    |   TYPE_WARNING { $res = new CursorProperty (CursorPropertyType.TypeWarning); }
    ;

declareTableDefinitions returns [List<CreateTableDefinition> res]
    :   dtd1=declareTableDefinition { $res = new List<CreateTableDefinition> { $dtd1.res }; }
        (COMMA dtd2=declareTableDefinition { $res.Add($dtd2.res); })*
    ;

declareTableDefinition returns [CreateTableDefinition res]
    :   columnDefinition { $res = $columnDefinition.res; }
    |   tableConstraint { $res = $tableConstraint.res; }
    ;
    
//OPEN { { [ GLOBAL ] cursor_name } | cursor_variable_name }
openCursorStatement returns [OpenCursorStatement res]
    :  OPEN cursorSource { $res = new OpenCursorStatement($cursorSource.res); }
    ;

cursorSource returns [CursorSource res]
    :  GLOBAL? statementEndIdentifier { $res = new CursorSource(($GLOBAL == null)? true : false, $statementEndIdentifier.res); }
    |  variable { $res = new CursorSource($variable.res); }
    ;

//FETCH 
//          [ [ NEXT | PRIOR | FIRST | LAST 
//                    | ABSOLUTE { n | @nvar } 
//                    | RELATIVE { n | @nvar } 
//               ] 
//               FROM 
//          ] 
//{ { [ GLOBAL ] cursor_name } | @cursor_variable_name } 
//[ INTO @variable_name [ ,...n ] ] 
fetchCursorStatement returns [FetchCursorStatement res]
    : FETCH fetchCursorOptions FROM cursorSource (INTO variableList)? { $res = new FetchCursorStatement($fetchCursorOptions.res, $cursorSource.res, $variableList.res); }
	| FETCH cursorSource (INTO variableList)? { $res = new FetchCursorStatement(null, $cursorSource.res, $variableList.res); }
    ;	 

variableList returns [List<VariableExpression> res]
	: va1=variable {$res = new List<VariableExpression> {$va1.res}; } 
		(COMMA va2=variable {$res.Add($va2.res); }
		)*
	;
    
fetchCursorOptions returns [List<FetchCursorOption> res]
    :  fo1=fetchCursorOption { $res = new List<FetchCursorOption> { $fo1.res }; }
    		(fo2=fetchCursorOption {$res.Add($fo2.res); }
    		)*
    ;	

fetchCursorOption returns [FetchCursorOption res]
   	: NEXT {$res = new FetchCursorOption(FetchCursorOptionType.NEXT); }
	| PRIOR {$res = new FetchCursorOption(FetchCursorOptionType.PRIOR); }
	| FIRST {$res = new FetchCursorOption(FetchCursorOptionType.FIRST); }
	| LAST {$res = new FetchCursorOption(FetchCursorOptionType.LAST); }
	| ABSOLUTE (constant | variable) {$res = new FetchCursorOption(FetchCursorOptionType.ABSOLUTE); }
 	| RELATIVE (constant | variable) {$res = new FetchCursorOption(FetchCursorOptionType.RELATIVE); }
	;

//DEALLOCATE { { [ GLOBAL ] cursor_name } | @cursor_variable_name }
deallocateStatement returns [DeallocateStatement res]
	: DEALLOCATE cursorSource {$res = new DeallocateStatement($cursorSource.res); }
	;

//CLOSE { { [ GLOBAL ] cursor_name } | cursor_variable_name }
closeStatement returns [CloseStatement res]
	: CLOSE cursorSource {$res = new CloseStatement($cursorSource.res); }
	;
	
setStatement returns [SetStatement res]
        // Note: No support for UDT and CLR types.
    :   SET variable assignmentOperator expression { $res = new SetStatement($variable.res, $assignmentOperator.res, $expression.res); }
    |   SET setOption onOff { $res = new SetSpecialStatement($setOption.res, $onOff.res ?? false); }
	|	SET DATEFORMAT setDateFormatType { $res = new SetDateFormatStatement($setDateFormatType.res); }
	|	SET DATEFORMAT variable { $res = new SetDateFormatVariableStatement( $variable.res); }
	|	SET DATEFIRST variable { $res = new SetDateFirstVariableStatement( $variable.res); } 
	|	SET DATEFIRST Integer { $res = new SetDateFirstStatement( Int32.Parse($Integer.Text)); }
	|	SET LOCK_TIMEOUT Integer { $res = new SetLockTimeoutStatement( Int32.Parse($Integer.Text)); }
	|	SET IDENTITY_INSERT dbObject onOff { $res = new SetIdentityInsertStatement( $dbObject.res, $onOff.res ?? false); }
	;
		// TODO: SET DEADLOCK_PRIORITY { LOW | NORMAL | HIGH | <numeric-priority> | @deadlock_var | @deadlock_intvar }     <numeric-priority> ::= { -10 | -9 | -8 | … | 0 | … | 8 | 9 | 10 }
		// TODO: SET FIPS_FLAGGER ( 'level' |  OFF )
		// TODO: SET LANGUAGE { [ N ] 'language' | @language_var } 
		// TODO: SET OFFSETS keyword_list { ON | OFF }
	    // TODO support cursor

setDateFormatType returns [SetDateFormatType res]
	: DMY { $res = SetDateFormatType.DMY; }
	| MDY { $res = SetDateFormatType.MDY; }
	| YMD { $res = SetDateFormatType.YMD; }
	| YDM { $res = SetDateFormatType.YDM; }
	| MYD { $res = SetDateFormatType.MYD; }
	| DYM { $res = SetDateFormatType.DYM; }
	;

setOption returns [SetOptionType res]
    : 	ANSI_WARNINGS { $res = (SetOptionType.ANSI_WARNINGS) ;}
	|	ANSI_NULLS { $res = (SetOptionType.ANSI_NULLS) ;}
	|	ANSI_DEFAULTS { $res = (SetOptionType.ANSI_DEFAULTS) ;}
	|	ANSI_PADDING { $res = (SetOptionType.ANSI_PADDING) ;}
	|	ANSI_NULL_DFLT_ON { $res = (SetOptionType.ANSI_NULL_DFLT_ON) ;}
	|	ANSI_NULL_DFLT_OFF { $res = (SetOptionType.ANSI_NULL_DFLT_OFF) ;}
	|	ARITHABORT { $res = (SetOptionType.ARITHABORT) ;}
	|	ARITHIGNORE { $res = (SetOptionType.ARITHIGNORE) ;}
	|	FMTONLY { $res = (SetOptionType.FMTONLY) ;}
	|	FORCEPLAN { $res = (SetOptionType.FORCEPLAN) ;}
	|	IMPLICIT_TRANSACTIONS { $res = (SetOptionType.IMPLICIT_TRANSACTIONS) ;}
	|	NOCOUNT { $res = (SetOptionType.NOCOUNT) ;}
	|	NOEXEC { $res = (SetOptionType.NOEXEC) ;}
	|	NUMERIC_ROUNDABORT { $res = (SetOptionType.NUMERIC_ROUNDABORT) ;}
	|	PARSEONLY { $res = (SetOptionType.PARSEONLY) ;}
	|	QUOTED_IDENTIFIER { $res = (SetOptionType.QUOTED_IDENTIFIER) ;}
	|	REMOTE_PROC_TRANSACTIONS { $res = (SetOptionType.REMOTE_PROC_TRANSACTIONS) ;}
	|	XACT_ABORT { $res = (SetOptionType.XACT_ABORT) ;}
	|	SHOWPLAN_ALL { $res = (SetOptionType.SHOWPLAN_ALL) ;}
	|	SHOWPLAN_TEXT { $res = (SetOptionType.SHOWPLAN_TEXT) ;}
	|	SHOWPLAN_XML { $res = (SetOptionType.SHOWPLAN_XML) ;}
    |	CONCAT_NULL_YIELDS_NULL { $res = (SetOptionType.CONCAT_NULL_YIELDS_NULL) ;}
	|	CURSOR_CLOSE_ON_COMMIT { $res = (SetOptionType.CURSOR_CLOSE_ON_COMMIT) ;}
    |   STATISTICS XML { $res = (SetOptionType.STATISTICS_XML) ;}
	|	STATISTICS IO { $res = (SetOptionType.STATISTICS_IO) ;}
	|	STATISTICS PROFILE { $res = (SetOptionType.STATISTICS_PROFILE) ;}
	|	STATISTICS TIME { $res = (SetOptionType.STATISTICS_TIME) ;}
    ;

// USE, PRINT, GO
useStatement returns [UseStatement res]
    :   USE identifier { $res = new UseStatement($identifier.res); }
    ;

printStatement returns [PrintStatement res]
    :   PRINT expression { $res = new PrintStatement($expression.res); }
    ;

goStatement returns [GoStatement res]
    :   GO Integer? { $res = new GoStatement($Integer == null ? 1 : Int32.Parse($Integer.Text)); }
    ;

// Transactions

beginTransactionStatement returns [BeginTransactionStatement res]
    :   BEGIN (TRAN | TRANSACTION) (transactionName (WITH MARK stringLiteral?)?)?
        {
            $res = new BeginTransactionStatement($transactionName.res, $MARK != null, $stringLiteral.res);
        }
    ;

commitTransactionStatement returns [CommitTransactionStatement res]
    :   COMMIT
        (
                (TRAN | TRANSACTION) transactionName?
            |   WORK
        )?
        {
            $res = new CommitTransactionStatement($transactionName.res);
        }
    ;

rollbackTransactionStatement returns [RollbackTransactionStatement res]
    :   ROLLBACK
        (
                (TRAN | TRANSACTION) transactionName?
            |   WORK
        )?
        {
            $res = new RollbackTransactionStatement($transactionName.res);
        }
    ;

saveTransactionStatement returns [SaveTransactionStatement res]
    :   SAVE (TRAN | TRANSACTION) transactionName?
        {
            $res = new SaveTransactionStatement($transactionName.res);
        }
    ;

transactionName returns [TransactionName res]
        // Since fromAsAlias can be at end of a statement, it has to use statementEndIdentifier instead of identifier.
    :   statementEndIdentifier { $res = new IdentifierTransactionName($statementEndIdentifier.res); }
    |   variable { $res = new VariableTransactionName($variable.res); }
    ;

// EXTERNAL NAME is not supported
createTypeStatement returns [CreateTypeStatement res]
    :   CREATE TYPE dbObject 
        (
                FROM dataType nullOrNotNull? { $res = new CreateBaseTypeStatement($dbObject.res, $dataType.res, $nullOrNotNull.res ?? true); }
            |   AS TABLE createTableDefinitions { $res = new CreateTableTypeStatement($dbObject.res, $createTableDefinitions.res); }
        )
    ;

dropTypeStatement returns [DropTypeStatement res]
    :   DROP TYPE dbObject { $res = new DropTypeStatement($dbObject.res); }
    ;

// Expression

// We need to distinguish generic expressions, and expressions that are outside of table context (= they cannot reference table columns).
// This is needed for IF and WHILE statements, where using full expression causes conflicts with dbObject and with NEXT VALUE FOR function.

// expression: Generic expression (union of wtExpression and ntExpression).
// ntExpression: Expression in a context where table columns are not allowed.

expression returns [Expression res]
    :   orExpression { $res = $orExpression.res; }
    ;

orExpression returns [Expression res]
    :   (ae1=andExpression { $res = $ae1.res; })
        (OR ae2=andExpression { $res = new BinaryLogicalExpression($res, BinaryLogicalOperatorType.Or, $ae2.res); })*
    ;

andExpression returns [Expression res]
    :   (ne1=notExpression { $res = $ne1.res; })
        (AND ne2=notExpression { $res = new BinaryLogicalExpression($res, BinaryLogicalOperatorType.And, $ne2.res); })*
    ;

notExpression returns [Expression res]
    :   NOT ne1=notExpression { $res = new LogicalNotExpression($ne1.res); }
    |   logicalExpression { $res = $logicalExpression.res; }
    ;

logicalExpression returns [Expression res]
    :   EXISTS LPAREN queryExpression RPAREN { $res = new ExistsExpression($queryExpression.res); }
    |   searchOperator LPAREN searchExpressionTarget COMMA stringLiteral (COMMA searchExpressionLanguage)? RPAREN
        {
            $res = new SearchExpression($searchOperator.res, $searchExpressionTarget.res, $stringLiteral.res, $searchExpressionLanguage.res);
        }
    |   ce1=comparisonExpression { $res = $ce1.res; }
        (
                comparisonOperator allAnyOperator LPAREN qe1=queryExpression RPAREN
                {
                    $res = new AllAnyExpression($ce1.res, $comparisonOperator.res, $allAnyOperator.res, $qe1.res);
                }
            |   n1=NOT?
                (
                        BETWEEN ce2=comparisonExpression AND ce3=comparisonExpression
                        {
                            $res = new BetweenExpression($ce1.res, $n1 != null, $ce2.res, $ce3.res);
                        }
                    |   (IN LPAREN SELECT) => IN LPAREN qe2=queryExpression RPAREN
                        {
                            $res = new InSubqueryExpression($ce1.res, $n1 != null, $qe2.res);
                        }
                    |   IN LPAREN inExpressionList RPAREN
                        {
                            $res = new InListExpression($ce1.res, $n1 != null, $inExpressionList.res);
                        }
                    |   LIKE ce4=comparisonExpression (ESCAPE ce5=comparisonExpression)?
                        {
                            $res = new LikeExpression($ce1.res, $n1 != null, $ce4.res, $ce5.res);
                        }
                )
            |   IS n2=NOT? NULL // only valid in WHERE/JOIN search condition
                {
                    $res = new IsNullExpression($ce1.res, $n2 != null);
                }
        )?
    ;

searchOperator returns [SearchOperatorType res]
    :   CONTAINS { $res = SearchOperatorType.Contains; }
    |   FREETEXT { $res = SearchOperatorType.FreeText; }
    ;

searchExpressionTarget returns [SearchExpressionTarget res]
    :   dbObject { $res = new SearchExpressionColumnTarget(new List<DbObject> { $dbObject.res }); }
    |   LPAREN dbObjectList RPAREN { $res = new SearchExpressionColumnTarget($dbObjectList.res); }
    |   ASTERISK { $res = new SearchExpressionWildcardTarget(); }
    ;

searchExpressionLanguage returns [SearchExpressionLanguage res]
    :   LANGUAGE stringLiteral { $res = new SearchExpressionStringLanguage($stringLiteral.res); }
    |   LANGUAGE Integer { $res = new SearchExpressionIntLanguage(Int32.Parse($Integer.Text)); }
    ;

allAnyOperator returns [AllAnyOperatorType res]
    :   ALL { $res = AllAnyOperatorType.All; }
    |   (SOME | ANY) { $res = AllAnyOperatorType.Any; }
    ;

inExpressionList returns [List<Expression> res]
    :   (e1=expression { $res = new List<Expression> { $e1.res }; })
        (COMMA e2=expression { $res.Add($e2.res); })*
    ;

comparisonExpression returns [Expression res]
    :   (abe1=addBitwiseExpression { $res = $abe1.res; })
        (comparisonOperator abe2=addBitwiseExpression { $res = new ComparisonExpression($res, $comparisonOperator.res, $abe2.res); })*
    ;

comparisonOperator returns [ComparisonOperatorType res]
    :   EQUAL { $res = ComparisonOperatorType.Equal; }
    |   notEqual { $res = ComparisonOperatorType.NotEqual; }
    |   lessThanOrEqual { $res = ComparisonOperatorType.LessThanOrEqual; }
    |   LESSTHAN { $res = ComparisonOperatorType.LessThan; }
    |   greaterThanOrEqual { $res = ComparisonOperatorType.GreaterThanOrEqual; }
    |   GREATERTHAN { $res = ComparisonOperatorType.GreaterThan; }
    ;

addBitwiseExpression returns [Expression res]
    :   (me1=multiplyExpression { $res = $me1.res; })
        (
                (binaryAddOperator me2=multiplyExpression { $res = new BinaryAddExpression($res, $binaryAddOperator.res, $me2.res); })
            |   (binaryBitwiseOperator me3=multiplyExpression { $res = new BinaryBitwiseExpression($res, $binaryBitwiseOperator.res, $me3.res); })
        )*
    ;

binaryAddOperator returns [BinaryAddOperatorType res]
    :   PLUS { $res = BinaryAddOperatorType.Plus; }
    |   MINUS { $res = BinaryAddOperatorType.Minus; }
    ;

binaryBitwiseOperator returns [BinaryBitwiseOperatorType res]
    :   AMPERSAND { $res = BinaryBitwiseOperatorType.And; }
    |   CHEVRON { $res = BinaryBitwiseOperatorType.Xor; }
    |   PIPE { $res = BinaryBitwiseOperatorType.Or; }
    ;

multiplyExpression returns [Expression res]
    :   (uao1=unaryAddExpression { $res = $uao1.res; })
        (multiplyOperator uao2=unaryAddExpression { $res = new MultiplyExpression($res, $multiplyOperator.res, $uao2.res); })*
    ;

multiplyOperator returns [MultiplyOperatorType res]
    :   ASTERISK { $res = MultiplyOperatorType.Multiply; }
    |   DIVIDE { $res = MultiplyOperatorType.Divide; }
    |   MODULO { $res = MultiplyOperatorType.Modulo; }
    ;

unaryAddExpression returns [Expression res]
    :   unaryAddOperator uae1=unaryAddExpression { $res = new UnaryAddExpression($unaryAddOperator.res, $uae1.res); }
    |   bitwiseNotExpression  { $res = $bitwiseNotExpression.res; }
    ;

unaryAddOperator returns [UnaryAddOperatorType res]
    :   PLUS { $res = UnaryAddOperatorType.Plus; }
    |   MINUS { $res = UnaryAddOperatorType.Minus; } 
    ;

bitwiseNotExpression returns [Expression res]
    :   TILDE bne1=bitwiseNotExpression { $res = new BitwiseNotExpression($bne1.res); }
    |   collationExpression { $res = $collationExpression.res; }
    ;

collationExpression returns [Expression res]
    :   basicExpression { $res = $basicExpression.res; }
        (collation { $res = new CollationExpression($basicExpression.res, $collation.res); })?
    ;

basicExpression returns [Expression res]
        // Conflict in SELECT ABS ((SELECT value FROM tab)). Is ABS column name and SELECT another statement, or is ABS a function?
    :   (genericFunctionIdentifier LPAREN) => genericScalarFunction { $res = $genericScalarFunction.res; }
    |   (specialScalarFunctionStart) => specialScalarFunction { $res = $specialScalarFunction.res; }
    |   builtinVariable { $res = new BuiltinVariableExpression($builtinVariable.res); }
    |   constant { $res = $constant.res; }
    |   dbObject { $res = new DbObjectExpression($dbObject.res); }
    |   variable { $res = $variable.res; }
    |   LPAREN
        (
                (selectStatement RPAREN) => selectStatement RPAREN { $res = new SubqueryExpression($selectStatement.res); }
            |   expression RPAREN { $res = new ParensExpression($expression.res); }
        )
    ;

variableOrConstant returns [Expression res]
    :   variable { $res = $variable.res; }
    |   constant { $res = $constant.res; }
    ;

variable returns [VariableExpression res]
    :   Variable { $res = new VariableExpression($Variable.text); }
    ;

// Non-table Expression

ntExpression returns [Expression res]
    :   ntOrExpression { $res = $ntOrExpression.res; }
    ;

ntOrExpression returns [Expression res]
    :   (ae1=ntAndExpression { $res = $ae1.res; })
        (OR ae2=ntAndExpression { $res = new BinaryLogicalExpression($res, BinaryLogicalOperatorType.Or, $ae2.res); })*
    ;

ntAndExpression returns [Expression res]
    :   (ne1=ntNotExpression { $res = $ne1.res; })
        (AND ne2=ntNotExpression { $res = new BinaryLogicalExpression($res, BinaryLogicalOperatorType.And, $ne2.res); })*
    ;

ntNotExpression returns [Expression res]
    :   NOT ne1=ntNotExpression { $res = new LogicalNotExpression($ne1.res); }
    |   ntLogicalExpression { $res = $ntLogicalExpression.res; }
    ;

ntLogicalExpression returns [Expression res]
    :   EXISTS LPAREN queryExpression RPAREN { $res = new ExistsExpression($queryExpression.res); }
    |   searchOperator LPAREN searchExpressionTarget COMMA stringLiteral (COMMA searchExpressionLanguage)? RPAREN
        {
            $res = new SearchExpression($searchOperator.res, $searchExpressionTarget.res, $stringLiteral.res, $searchExpressionLanguage.res);
        }
    |   ce1=ntComparisonExpression { $res = $ce1.res; }
        (
                comparisonOperator allAnyOperator LPAREN qe1=queryExpression RPAREN
                {
                    $res = new AllAnyExpression($ce1.res, $comparisonOperator.res, $allAnyOperator.res, $qe1.res);
                }
            |   n1=NOT?
                (
                        BETWEEN ce2=ntComparisonExpression AND ce3=ntComparisonExpression
                        {
                            $res = new BetweenExpression($ce1.res, $n1 != null, $ce2.res, $ce3.res);
                        }
                    |   (IN LPAREN SELECT) => IN LPAREN qe2=queryExpression RPAREN
                        {
                            $res = new InSubqueryExpression($ce1.res, $n1 != null, $qe2.res);
                        }
                    |   IN LPAREN inExpressionList RPAREN
                        {
                            $res = new InListExpression($ce1.res, $n1 != null, $inExpressionList.res);
                        }
                    |   LIKE ce4=ntComparisonExpression (ESCAPE ce5=ntComparisonExpression)?
                        {
                            $res = new LikeExpression($ce1.res, $n1 != null, $ce4.res, $ce5.res);
                        }
                )
            |   IS n2=NOT? NULL // only valid in WHERE/JOIN search condition
                {
                    $res = new IsNullExpression($ce1.res, $n2 != null);
                }
        )?
    ;

ntComparisonExpression returns [Expression res]
    :   (abe1=ntAddBitwiseExpression { $res = $abe1.res; })
        (comparisonOperator abe2=ntAddBitwiseExpression { $res = new ComparisonExpression($res, $comparisonOperator.res, $abe2.res); })*
    ;

ntAddBitwiseExpression returns [Expression res]
    :   (me1=ntMultiplyExpression { $res = $me1.res; })
        (
                (binaryAddOperator me2=ntMultiplyExpression { $res = new BinaryAddExpression($res, $binaryAddOperator.res, $me2.res); })
            |   (binaryBitwiseOperator me3=ntMultiplyExpression { $res = new BinaryBitwiseExpression($res, $binaryBitwiseOperator.res, $me3.res); })
        )*
    ;

ntMultiplyExpression returns [Expression res]
    :   (uao1=ntUnaryAddExpression { $res = $uao1.res; })
        (multiplyOperator uao2=ntUnaryAddExpression { $res = new MultiplyExpression($res, $multiplyOperator.res, $uao2.res); })*
    ;

ntUnaryAddExpression returns [Expression res]
    :   unaryAddOperator uae1=ntUnaryAddExpression { $res = new UnaryAddExpression($unaryAddOperator.res, $uae1.res); }
    |   ntBitwiseNotExpression  { $res = $ntBitwiseNotExpression.res; }
    ;

ntBitwiseNotExpression returns [Expression res]
    :   TILDE bne1=ntBitwiseNotExpression { $res = new BitwiseNotExpression($bne1.res); }
    |   ntCollationExpression { $res = $ntCollationExpression.res; }
    ;

ntCollationExpression returns [Expression res]
    :   ntBasicExpression { $res = $ntBasicExpression.res; }
        (collation { $res = new CollationExpression($ntBasicExpression.res, $collation.res); })?
    ;

ntBasicExpression returns [Expression res]
        // Conflict in SELECT ABS ((SELECT value FROM tab)). Is ABS column name or function?
    :   (genericFunctionIdentifier LPAREN) => genericScalarFunction { $res = $genericScalarFunction.res; }
    |   ntSpecialScalarFunction { $res = $ntSpecialScalarFunction.res; }
    |   builtinVariable { $res = new BuiltinVariableExpression($builtinVariable.res); }
    |   constant { $res = $constant.res; }
    |   variable { $res = $variable.res; }
    |   LPAREN
        (
                (selectStatement RPAREN) => selectStatement RPAREN { $res = new SubqueryExpression($selectStatement.res); }
            |   ntExpression RPAREN { $res = new ParensExpression($ntExpression.res); }
        )
    ;

// FUNCTIONS

builtinVariable returns [BuiltinVariableType res]
        // Configuration Functions
    :   F_DATEFIRST { $res = BuiltinVariableType.DATEFIRST; }
    |   F_DBTS { $res = BuiltinVariableType.DBTS; }
    |   F_LANGID { $res = BuiltinVariableType.LANGID; }
    |   F_LANGUAGE { $res = BuiltinVariableType.LANGUAGE; }
    |   F_LOCK_TIMEOUT { $res = BuiltinVariableType.LOCK_TIMEOUT; }
    |   F_MAX_CONNECTIONS { $res = BuiltinVariableType.MAX_CONNECTIONS; }
    |   F_MAX_PRECISION { $res = BuiltinVariableType.MAX_PRECISION; }
    |   F_NESTLEVEL { $res = BuiltinVariableType.NESTLEVEL; }
    |   F_OPTIONS { $res = BuiltinVariableType.OPTIONS; }
    |   F_REMSERVER { $res = BuiltinVariableType.REMSERVER; }
    |   F_SERVERNAME { $res = BuiltinVariableType.SERVERNAME; }
    |   F_SERVICENAME { $res = BuiltinVariableType.SERVICENAME; }
    |   F_SPID { $res = BuiltinVariableType.SPID; }
    |   F_TEXTSIZE { $res = BuiltinVariableType.TEXTSIZE; }
    |   F_VERSION { $res = BuiltinVariableType.VERSION; }
        // Cursor Functions
    |   F_CURSOR_ROWS { $res = BuiltinVariableType.CURSOR_ROWS; }
    |   F_FETCH_STATUS { $res = BuiltinVariableType.FETCH_STATUS; }
        // Metadata Functions
    |   F_PROCID { $res = BuiltinVariableType.PROCID; }
        // System Functions
    |   F_ERROR { $res = BuiltinVariableType.ERROR; }
    |   F_IDENTITY { $res = BuiltinVariableType.IDENTITY; }
    |   F_ROWCOUNT { $res = BuiltinVariableType.ROWCOUNT; }
    |   F_TRANCOUNT { $res = BuiltinVariableType.TRANCOUNT; }
        // System Statistical Functions
    |   F_CONNECTIONS { $res = BuiltinVariableType.CONNECTIONS; }
    |   F_CPU_BUSY { $res = BuiltinVariableType.CPU_BUSY; }
    |   F_IDLE { $res = BuiltinVariableType.IDLE; }
    |   F_IO_BUSY { $res = BuiltinVariableType.IO_BUSY; }
    |   F_PACKET_ERRORS { $res = BuiltinVariableType.PACKET_ERRORS; }
    |   F_PACK_RECEIVED { $res = BuiltinVariableType.PACK_RECEIVED; }
    |   F_PACK_SENT { $res = BuiltinVariableType.PACK_SENT; }
    |   F_TIMETICKS { $res = BuiltinVariableType.TIMETICKS; }
    |   F_TOTAL_ERRORS { $res = BuiltinVariableType.TOTAL_ERRORS; }
    |   F_TOTAL_READ { $res = BuiltinVariableType.TOTAL_READ; }
    |   F_TOTAL_WRITE { $res = BuiltinVariableType.TOTAL_WRITE; }
    ;

// The genericScalarFunction rule handles user-defined functions, as well as built-in functions that do not
// require special grammar constructs. Following built-in functions are handled as generic functions:
//
// Cursor Functions
// CURSOR_STATUS (2 args)
//
// Logical Functions
// CHOOSE (3 or more args)
// IIF (3 args)
//
// Date and Time Functions
// SYSDATETIME, SYSDATETIMEOFFSET, SYSUTCDATETIME, GETDATE, GETUTCDATE (0 args)
// DAY, MONTH, YEAR, ISDATE (1 arg)
// EOMONTH (1 or 2 args)
// SWITCHOFFSET, TODATETIMEOFFSET (2 args)
// DATEFROMPARTS, DATEDIFF, DATEADD (3 args)
// SMALLDATETIMEFROMPARTS, TIMEFROMPARTS  (5 args)
// DATETIMEFROMPARTS (7 args)
// DATETIME2FROMPARTS, DATETIMEOFFSETFROMPARTS (8 args)
//
// Mathematical Functions
// PI (0 args)
// RAND (0 or 1 arg)
// ABS, ACOS, ASIN, ATAN, CEILING, COS, COT, DEGREES, EXP, FLOOR, LOG10, RADIANS, SIGN, SIN, SQRT, SQUARE, TAN (1 arg)
// LOG (1 or 2 args)
// ATN2, POWER (2 args)
// ROUND (2 or 3 args)
//
// Metadata Functions
// APP_NAME, ORIGINAL_DB_NAME, SCOPE_IDENTITY (0 args)
// DB_ID, DB_NAME, SCHEMA_ID, SCHEMA_NAME (0 or 1 arg)
// DATABASE_PRINCIPAL_ID, FILE_ID, FILE_IDEX, FILE_NAME, FILEGROUP_ID, FILEGROUP_NAME, FULLTEXTSERVICEPROPERTY, OBJECT_DEFINITION,
// SERVERPROPERTY, TYPE_ID, TYPE_NAME (1 arg)
// OBJECT_ID, OBJECT_NAME, OBJECT_SCHEMA_NAME (1 or 2 args)
// ASSEMBLYPROPERTY, COL_LENGTH, COL_NAME, DATABASEPROPERTYEX, FILEGROUPPROPERTY, FILEPROPERTY, FULLTEXTCATALOGPROPERTY, 
// OBJECTPROPERTY, OBJECTPROPERTYEX, PARSENAME, STATS_DATE, TYPEPROPERTY (2 args)
// APPLOCK_MODE, COLUMNPROPERTY, INDEX_COL, INDEXPROPERTY (3 args)
// APPLOCK_TEST, INDEXKEY_PROPERTY (4 args)
//
// Security Functions
// ORIGINAL_LOGIN, SCHEMA_ID, SCHEMA_NAME (0 args)
// SUSER_ID, SUSER_SNAME, SUSER_NAME, USER_ID, USER_NAME (0 or 1 arg)
// PERMISSIONS, SUSER_SID (0 to 2 args)
// CERTENCODED, DATABASE_PRINCIPAL_ID, IS_MEMBER, PWDENCRYPT (1 arg)
// IS_ROLEMEMBER, IS_SRVROLEMEMBER,  (1 or 2 args)
// CERTPRIVATEKEY, PWDCOMPARE (2 or 3 args)
// HAS_PERMS_BY_NAME (3 to 5 args)
//
// String Functions
// ASCII, CHAR, LEN, LOWER, LTRIM, NCHAR, REVERSE, RTRIM, SOUNDEX, SPACE, UNICODE, UPPER (1 arg)
// QUOTENAME (1 or 2 args)
// STR (1 to 3 args)
// DIFFERENCE, LEFT PATINDEX, REPLICATE, RIGHT (2 args)
// CHARINDEX, CONCAT, FORMAT (2 or 3 args)
// REPLACE, SUBSTRING (3 args)
// STUFF (4 args)
//
// System Functions
// CONTEXT_INFO, CURRENT_REQUEST_ID, ERROR_LINE, ERROR_MESSAGE, ERROR_NUMBER, ERROR_PROCEDURE, ERROR_SEVERITY, ERROR_STATE, 
// GET_FILESTREAM_TRANSACTION_CONTEXT, HOST_ID, HOST_NAME, NEWID, NEWSEQUENTIALID, ROWCOUNT_BIG, XACT_STATE (0 args)
// GETANSINULL (0 or 1 arg)
// CONNECTIONPROPERTY, ISNUMERIC (1 arg)
// FORMATMESSAGE (1 or more args)
// ISNULL (2 args)
//
// Text Functions
// TEXTPTR (1 arg)
// TEXTVALID (2 args)
//
// Collation Functions
// TERTIARY_WEIGHTS (1 arg)
// COLLATIONPROPERTY (2 args)
//
// Cryptographic Functions
// KEY_ID, KEY_GUID, CERT_ID, ASYMKEY_ID, CERTENCODED (1 arg)
// DECRYPTBYKEY (1 to 3 args)
// ENCRYPTBYASYMKEY, ENCRYPTBYCERT, CERTPROPERTY, HASHBYTES (2 args)
// DECRYPTBYASYMKEY, DECRYPTBYCERT, SIGNBYASYMKEY, SIGNBYCERT, CERTPRIVATEKEY (2 to 3 args)
// ENCRYPTBYKEY, ENCRYPTBYPASSPHRASE, DECRYPTBYPASSPHRASE (2 to 4 args)
// VERIFYSIGNEDBYASYMKEY, VERIFYSIGNEDBYCERT (3 args)
// DECRYPTBYKEYAUTOCERT (3 to 5 args)
//
// Data Type Functions
// DATALENGTH, IDENT_CURRENT, IDENT_INCR, IDENT_SEED (1 arg)
// SQL_VARIANT_PROPERTY (2 args)
//
// Replication Functions
// PUBLISHINGSERVERNAME (0 args)
//
// Trigger Functions
// COLUMNS_UPDATED, EVENTDATA (0 args)
// UPDATE (1 arg)
// TRIGGER_NESTLEVEL (0 to 3 args)
//
// Aggregate Functions
// GROUPING (1 arg)
// GROUPING_ID (1 or more args)
//
// Other Functions
// COALESCE (1 or more args)
// NULLIF (2 args)

// Missing support for following functions:
//
// Date and Time Functions
// SET LANGUAGE { [ N ] 'language' | @language_var }
// sp_helplanguage [ [ @language = ] 'language' ]
// EXTRACT( extract-field FROM extract-source) (ODBC 3.0)
//
// System Functions
// [ database_name. ] $PARTITION.partition_function_name(expression)

genericScalarFunction returns [GenericScalarFunctionExpression res]
    :   genericFunctionIdentifier LPAREN genericFunctionArguments? RPAREN
        {
            $res = new GenericScalarFunctionExpression($genericFunctionIdentifier.res, $genericFunctionArguments.res);
        }
    ;

genericFunctionArguments returns [List<Expression> res]
    :   (e1=expression { $res = new List<Expression> { $e1.res }; })
        (COMMA e2=expression { $res.Add($e2.res); })*
    ;

// See also specialScalarFunctionStart.
specialScalarFunction returns [Expression res]
    :   ntSpecialScalarFunction { $res = $ntSpecialScalarFunction.res; }
    |   nextValueForScalarFunction { $res = $nextValueForScalarFunction.res; }
    ;

// This is used only in predicate, to decide whether a special scalar function follows.
specialScalarFunctionStart
    :   ntSpecialScalarFunctionStart
    |   NEXT VALUE FOR
    ;

// See also ntSpecialScalarFunctionStart.
ntSpecialScalarFunction returns [Expression res]
        // Conversion functions
    :   CAST LPAREN expression AS dataType RPAREN
        {
            $res = new CastFunctionExpression($expression.res, $dataType.res);
        }
    |   convertOrTryConvert LPAREN dataType COMMA expression (COMMA Integer)? RPAREN
        {
            $res = new ConvertFunctionExpression($convertOrTryConvert.res, $dataType.res, $expression.res, $Integer == null ? 0 : Int32.Parse($Integer.Text));
        }
    |   parseOrTryParse LPAREN e1=expression AS identifier (USING e2=expression)? RPAREN
        {
            $res = new ParseFunctionExpression($parseOrTryParse.res, $e1.res, $identifier.res, $e2.res);
        }
        // Date and Time Functions
    |   CURRENT_TIMESTAMP
        {
            $res = new ParameterlessFunctionExpression(ParameterlessFunctionType.CurrentTimestamp);
        }
    |   DATENAME LPAREN identifier COMMA expression RPAREN
        {
            $res = new DatenameFunctionExpression($identifier.res, $expression.res);
        }
    |   DATEPART LPAREN identifier COMMA expression RPAREN
        {
            $res = new DatepartFunctionExpression($identifier.res, $expression.res);
        }
        // Security Functions
    |   CURRENT_USER
        {
            $res = new ParameterlessFunctionExpression(ParameterlessFunctionType.CurrentUser);
        }
    |   SESSION_USER
        {
            $res = new ParameterlessFunctionExpression(ParameterlessFunctionType.SessionUser);
        }
    |   SYSTEM_USER
        {
            $res = new ParameterlessFunctionExpression(ParameterlessFunctionType.SystemUser);
        }
        // System Functions
    |   checksumOrBinaryChecksum LPAREN asteriskOrExpressionList RPAREN
        {
            $res = new ChecksumFunctionExpression($checksumOrBinaryChecksum.res, $asteriskOrExpressionList.res);
        }
    |   MIN_ACTIVE_ROWVERSION
        {
            $res = new ParameterlessFunctionExpression(ParameterlessFunctionType.MinActiveRowVersion);
        }
    |   IIF LPAREN e1=expression COMMA e2=expression COMMA e3=expression RPAREN
        {
            $res = new IifFunctionExpression($e1.res, $e2.res, $e3.res);
        }
    |   CHOOSE LPAREN e4=expression COMMA e5=expression COMMA genericFunctionArguments RPAREN
        {
            $res = new ChooseFunctionExpression($e4.res, $e5.res, $genericFunctionArguments.res);
        }
        // Note: CASE is not really a function according to MSDN, but it seems to behave like one.
    |   CASE expression? caseWhenClauses caseElseClause? END
        {
            $res = new CaseFunctionExpression($expression.res, $caseWhenClauses.res, $caseElseClause.res);
        }
        // Data Type Functions
    |   IDENTITY LPAREN dataType (COMMA i1=Integer COMMA i2=Integer)? RPAREN
        {
            $res = new IdentityFunctionExpression($dataType.res, 
                $i1 == null ? 0 : Int32.Parse($i1.Text), $i2 == null ? 0 : Int32.Parse($i2.Text));
        }
        // Ranking Functions
    |   rankingFunctionType LPAREN RPAREN overClause
        {
            $res = new RankingFunctionExpression($rankingFunctionType.res, $overClause.res);
        }
    |   NTILE LPAREN expression RPAREN overClause
        {
            $res = new NTileFunctionExpression($expression.res, $overClause.res);
        }
        // Aggregate Functions
    |   simpleAggregateFunctionType LPAREN distinctClause? expression RPAREN overClause?
        {
            $res = new SimpleAggregateFunctionExpression($simpleAggregateFunctionType.res, $distinctClause.res, $expression.res, $overClause.res);
        }
    |   countFunctionType LPAREN
        (
                distinctClause? expression RPAREN oc1=overClause?
                {
                    $res = new ExpressionCountFunctionExpression($countFunctionType.res, $distinctClause.res, $expression.res, $oc1.res);
                }
            |   ASTERISK RPAREN oc2=overClause?
                {
                    $res = new AsteriskCountFunctionExpression($countFunctionType.res, $oc2.res);
                }
        )
        // Analytic Functions
    |   cumePercentFunctionType LPAREN RPAREN overClause
        {
            $res = new CumePercentFunctionExpression($cumePercentFunctionType.res, $overClause.res);
        }
    |   firstLastValueFunctionType LPAREN expression? RPAREN overClause
        {
            $res = new FirstLastValueFunctionExpression($firstLastValueFunctionType.res, $expression.res, $overClause.res);
        }
    |   lagLeadFunctionType LPAREN e1=expression (COMMA e2=expression (COMMA e3=expression)?)? RPAREN overClause
        {
            $res = new LagLeadFunctionExpression($lagLeadFunctionType.res, $e1.res, $e2.res, $e3.res, $overClause.res);
        }
    |   percentileContDiscFunctionType LPAREN decimalLiteral RPAREN WITHIN GROUP LPAREN ORDER BY expression orderDirection? RPAREN overClause
        {
            $res = new PercentileContDiscFunctionExpression($percentileContDiscFunctionType.res, 
                $decimalLiteral.res, $expression.res, $orderDirection.res, $overClause.res);
        }
    ;

// This is used only in predicate, to decide whether a special scalar function follows.
ntSpecialScalarFunctionStart
    :   (
                CAST | convertOrTryConvert | parseOrTryParse | DATENAME | DATEPART | checksumOrBinaryChecksum | IDENTITY
            |   rankingFunctionType | NTILE | simpleAggregateFunctionType | countFunctionType | cumePercentFunctionType
            |   firstLastValueFunctionType | lagLeadFunctionType | percentileContDiscFunctionType | IIF | CHOOSE
        )
        LPAREN
    |   CURRENT_TIMESTAMP | CURRENT_USER | SESSION_USER | SYSTEM_USER | MIN_ACTIVE_ROWVERSION
    |   CASE expression? (WHEN | ELSE)
    ;

nextValueForScalarFunction returns [Expression res]
        // Metadata Functions
    :   NEXT VALUE FOR dbObject overClause?
        {
            $res = new NextValueFunctionExpression($dbObject.res, $overClause.res);
        }
    ;

convertOrTryConvert returns [bool res]
    :   CONVERT { $res = false; }
    |   TRY_CONVERT { $res = true; }
    ;

parseOrTryParse returns [bool res]
    :   PARSE { $res = false; }
    |   TRY_PARSE { $res = true; }
    ;

checksumOrBinaryChecksum returns [bool res]
    :   CHECKSUM { $res = false; }
    |   BINARY_CHECKSUM { $res = true; }
    ;

asteriskOrExpressionList returns [List<Expression> res]
    :   ASTERISK { $res = new List<Expression>(); }
    |   (e1=expression { $res = new List<Expression> { $e1.res }; })
        (COMMA e2=expression { $res.Add($e2.res); })*
    ;

caseWhenClauses returns [List<CaseWhenClause> res]
    :   (wc1=caseWhenClause { $res = new List<CaseWhenClause> { $wc1.res }; })
        (wc2=caseWhenClause { $res.Add($wc2.res); })*
    ;

caseWhenClause returns [CaseWhenClause res]
    :   WHEN e1=expression THEN e2=expression { $res = new CaseWhenClause($e1.res, $e2.res); }
    ;

caseElseClause returns [Expression res]
    :   ELSE expression { $res = $expression.res; }
    ;

rankingFunctionType returns [RankingFunctionType res]
    :   RANK { $res = RankingFunctionType.Rank; }
    |   DENSE_RANK { $res = RankingFunctionType.DenseRank; }
    |   ROW_NUMBER { $res = RankingFunctionType.RowNumber; }
    ;

overClause returns [OverClause res]
    :   OVER LPAREN partitionByClause? orderByClause? rowsRangeClause? RPAREN
        {
            $res = new OverClause($partitionByClause.res, $orderByClause.res, $rowsRangeClause.res);
        }
    ;

partitionByClause returns [List<Expression> res]
    :   PARTITION BY
        (e1=expression { $res = new List<Expression> { $e1.res }; })
        (COMMA e2=expression { $res.Add($e2.res); })*
    ;

rowsRangeClause returns [RowsRangeClause res]
    :   rowsOrRange windowFrameExtent { $res = new RowsRangeClause($rowsOrRange.res, $windowFrameExtent.res); }
    ;

rowsOrRange returns [RowsOrRangeType res]
    :   ROWS { $res = RowsOrRangeType.Rows; }
    |   RANGE { $res = RowsOrRangeType.Range; }
    ;

windowFrameExtent returns [WindowFrameExtent res]
    :   windowFramePreceding { $res = $windowFramePreceding.res; }
    |   windowFrameBetween { $res = $windowFrameBetween.res; }
    ;

windowFrameBetween returns [WindowFrameExtent res]
    :   BETWEEN wfb1=windowFrameBound AND wfb2=windowFrameBound { $res = new BetweenWindowFrameExtent($wfb1.res, $wfb2.res); }
    ;

windowFrameBound returns [WindowFrameExtent res]
    :   windowFramePreceding { $res = $windowFramePreceding.res; }
    |   windowFrameFollowing { $res = $windowFrameFollowing.res; }
    ;

windowFramePreceding returns [WindowFrameExtent res]
    :   UNBOUNDED PRECEDING { $res = new SimpleWindowFrameExtent(SimpleWindowFrameExtentType.UnboundedPreceding); }
    |   Integer PRECEDING { $res = new IntegerWindowFrameExtent(Int32.Parse($Integer.Text), IntegerWindowFrameExtentType.Preceding); }
    |   CURRENT ROW { $res = new SimpleWindowFrameExtent(SimpleWindowFrameExtentType.CurrentRow); }
    ;

windowFrameFollowing returns [WindowFrameExtent res]
    :   UNBOUNDED FOLLOWING { $res = new SimpleWindowFrameExtent(SimpleWindowFrameExtentType.UnboundedFollowing); }
    |   Integer FOLLOWING { $res = new IntegerWindowFrameExtent(Int32.Parse($Integer.Text), IntegerWindowFrameExtentType.Following); }
    ;

simpleAggregateFunctionType returns [SimpleAggregateFunctionType res]
    :   AVG { $res = SimpleAggregateFunctionType.Avg; }
    |   CHECKSUM_AGG { $res = SimpleAggregateFunctionType.ChecksumAgg; }
    |   MAX { $res = SimpleAggregateFunctionType.Max; }
    |   MIN { $res = SimpleAggregateFunctionType.Min; }
    |   SUM { $res = SimpleAggregateFunctionType.Sum; }
    |   STDEV { $res = SimpleAggregateFunctionType.StDev; }
    |   STDEVP { $res = SimpleAggregateFunctionType.StDevP; }
    |   VAR { $res = SimpleAggregateFunctionType.Var; }
    |   VARP { $res = SimpleAggregateFunctionType.VarP; }
    ;

countFunctionType returns [CountFunctionType res]
    :   COUNT { $res = CountFunctionType.Count; }
    |   COUNT_BIG { $res = CountFunctionType.CountBig; }
    ;

cumePercentFunctionType returns [CumePercentFunctionType res]
    :   CUME_DIST { $res = CumePercentFunctionType.CumeDist; }
    |   PERCENT_RANK { $res = CumePercentFunctionType.PercentRank; }
    ;

firstLastValueFunctionType returns [FirstLastValueFunctionType res]
    :   FIRST_VALUE { $res = FirstLastValueFunctionType.FirstValue; }
    |   LAST_VALUE { $res = FirstLastValueFunctionType.LastValue; }
    ;

lagLeadFunctionType returns [LagLeadFunctionType res]
    :   LAG { $res = LagLeadFunctionType.Lag; }
    |   LEAD { $res = LagLeadFunctionType.Lead; }
    ;

percentileContDiscFunctionType returns [PercentileContDiscFunctionType res]
    :   PERCENTILE_CONT { $res = PercentileContDiscFunctionType.PercentileCont; }
    |   PERCENTILE_DISC { $res = PercentileContDiscFunctionType.PercentileDisc; }
    ;

// Row set functions return table values, not scalar values.
rowsetFunction returns [RowsetFunction res]
    :   openDataSourceFunction { $res = $openDataSourceFunction.res; }
    |   openQueryFunction { $res = $openQueryFunction.res; }
    |   openRowsetFunction { $res = $openRowsetFunction.res; }
    |   openXmlFunction { $res = $openXmlFunction.res; }
    ;

openDataSourceFunction returns [OpenDataSourceFunction res]
    :   OPENDATASOURCE LPAREN e1=expression COMMA e2=expression RPAREN DOT dbObject
        {
            $res = new OpenDataSourceFunction($e1.res, $e2.res, $dbObject.res);
        }
    ;

openQueryFunction returns [OpenQueryFunction res]
    :   OPENQUERY LPAREN identifier COMMA expression RPAREN
        {
            $res = new OpenQueryFunction($identifier.res, $expression.res);
        }
    ;

openRowsetFunction returns [RowsetFunction res]
    :   OPENROWSET LPAREN
        (
                sl1=stringLiteral COMMA openRowsetProviderSpec COMMA openRowsetSource
                {
                    $res = new OpenRowsetQueryFunction($sl1.res, $openRowsetProviderSpec.res, $openRowsetSource.res);
                }
            |   BULK sl2=stringLiteral COMMA openRowsetBulkFormat
                {
                    $res = new OpenRowsetBulkFunction($sl2.res, $openRowsetBulkFormat.res);
                }
        )
        RPAREN
    ;

openRowsetProviderSpec returns [OpenRowsetProviderSpec res]
    :   sl1=stringLiteral SEMICOLON sl2=stringLiteral SEMICOLON sl3=stringLiteral
        {
            $res = new StructuredOpenRowsetProviderSpec($sl1.res, $sl2.res, $sl3.res);
        }
    |   sl4=stringLiteral
        {
            $res = new StringOpenRowsetProviderSpec($sl4.res);
        }
    ;

openRowsetSource returns [OpenRowsetSource res]
    :   dbObject { $res = new DbObjectOpenRowsetSource($dbObject.res); }
    |   stringLiteral { $res = new QueryOpenRowsetSource($stringLiteral.res); }
    ;

openRowsetBulkFormat returns [OpenRowsetBulkFormat res]
    :   FORMATFILE EQUAL stringLiteral withBulkOptions? { $res = new FormatFileOpenRowsetBulkFormat($stringLiteral.res, $withBulkOptions.res); }
    |   SINGLE_BLOB { $res = new LobOpenRowsetBulkFormat(LobType.Blob); }
    |   SINGLE_CLOB { $res = new LobOpenRowsetBulkFormat(LobType.Clob); }
    |   SINGLE_NCLOB { $res = new LobOpenRowsetBulkFormat(LobType.Nclob); }
    ;

withBulkOptions returns [List<BulkOption> res]
    :   WITH
        bo1=bulkOption { $res = new List<BulkOption> { $bo1.res }; }
        (COMMA bo2=bulkOption { $res.Add($bo2.res); })*
    ;

bulkOption returns [BulkOption res]
    :   CODEPAGE EQUAL stringLiteral { $res = new StringBulkOption(StringBulkOptionType.CodePage, $stringLiteral.res); }
    |   ERRORFILE EQUAL stringLiteral { $res = new StringBulkOption(StringBulkOptionType.ErrorFile, $stringLiteral.res); }
    |   FIRSTROW EQUAL Integer { $res = new IntBulkOption(IntBulkOptionType.FirstRow, Int32.Parse($Integer.Text)); }
    |   LASTROW EQUAL Integer { $res = new IntBulkOption(IntBulkOptionType.LastRow, Int32.Parse($Integer.Text)); }
    |   MAXERRORS EQUAL Integer { $res = new IntBulkOption(IntBulkOptionType.MaxErrors, Int32.Parse($Integer.Text)); }
    |   ROWS_PER_BATCH EQUAL Integer { $res = new IntBulkOption(IntBulkOptionType.RowsPerBatch, Int32.Parse($Integer.Text)); }
    |   ORDER orderedColumnsInParens UNIQUE? { $res = new OrderBulkOption($orderedColumnsInParens.res, $UNIQUE != null); }
    ;

openXmlFunction returns [OpenXmlFunction res]
    :   OPENXML LPAREN e1=expression COMMA e2=expression (COMMA e3=expression)? RPAREN openXmlWithClause?
        {
            $res = new OpenXmlFunction($e1.res, $e2.res, $e3.res, $openXmlWithClause.res);
        }
    ;

openXmlWithClause returns [OpenXmlWithClause res]
    :   WITH LPAREN
        (
                openXmlColumnSpecs RPAREN { $res = new ColumnSpecOpenXmlWithClause($openXmlColumnSpecs.res); }
            |   dbObject RPAREN { $res = new TableOpenXmlWithClause($dbObject.res); }
        )
    ;

openXmlColumnSpecs returns [List<OpenXmlColumnSpec> res]
    :   oxcs1=openXmlColumnSpec { $res = new List<OpenXmlColumnSpec> { $oxcs1.res }; }
        (COMMA oxcs2=openXmlColumnSpec { $res.Add($oxcs2.res); })*
    ;

openXmlColumnSpec returns [OpenXmlColumnSpec res]
    :   identifier dataType stringLiteral? { $res = new OpenXmlColumnSpec($identifier.res, $dataType.res, $stringLiteral.res); }
    ;

// Security Functions
// following are not scalar functions: fn_builtin_permissions, fn_get_audit_file, fn_my_permissions

// System Statistical Functions
// following are not scalar functions: fn_virtualfilestats
// fn_virtualfilestats ( { database_id | NULL } , { file_id | NULL } )

// DATA TYPES

dataType returns [DataType res]
    :   builtinDataType { $res = $builtinDataType.res; }
    |   genericDataType { $res = $genericDataType.res; }
    ;

builtinDataType returns [BuiltinDataType res]
    :   simpleBuiltinDataType { $res = $simpleBuiltinDataType.res; }
    |   decimalDataType { $res = $decimalDataType.res; }
    |   floatDataType { $res = $floatDataType.res; }
    |   dateTimePrecisionDataType { $res = $dateTimePrecisionDataType.res; }
    |   stringDataType { $res = $stringDataType.res; }
    |   variableStringDataType { $res = $variableStringDataType.res; }
    ;

simpleBuiltinDataType returns [SimpleBuiltinDataType res]
        // Exact Numerics
    :   (BIGINT | BR_BIGINT | QT_BIGINT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.BigInt); }
    |   (BIT | BR_BIT | QT_BIT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Bit); }
    |   (INT | BR_INT | QT_INT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Int); }
    |   (INTEGER | BR_INTEGER | QT_INTEGER) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Int); }
    |   (MONEY | BR_MONEY | QT_MONEY) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Money); }
    |   (SMALLINT | BR_SMALLINT | QT_SMALLINT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.SmallInt); }
    |   (SMALLMONEY | BR_SMALLMONEY | QT_SMALLMONEY) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.SmallMoney); }
    |   (TINYINT | BR_TINYINT | QT_TINYINT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.TinyInt); }
        // Approximate Numerics
    |   (REAL | BR_REAL | QT_REAL) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Real); }
        // Date and Time
    |   (DATE | BR_DATE | QT_DATE) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Date); }
    |   (DATETIME | BR_DATETIME | QT_DATETIME) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.DateTime); }
    |   (SMALLDATETIME | BR_SMALLDATETIME | QT_SMALLDATETIME) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.SmallDateTime); }
    |   (TEXT | BR_TEXT | QT_TEXT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Text); }
    |   (NTEXT | BR_NTEXT | QT_NTEXT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.NText); }
    |   NATIONAL (TEXT | BR_TEXT | QT_TEXT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.NText); }
    |   (IMAGE | BR_IMAGE | QT_IMAGE) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Image); }
        // Other Data Types
    |   (HIERARCHYID | BR_HIERARCHYID | QT_HIERARCHYID) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.HierarchyId); }
    |   (ROWVERSION | BR_ROWVERSION | QT_ROWVERSION) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.RowVersion); }
    |   (SQL_VARIANT | BR_SQL_VARIANT | QT_SQL_VARIANT) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.SqlVariant); }
    |   (TIMESTAMP | BR_TIMESTAMP | QT_TIMESTAMP) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.TimeStamp); }
    |   (UNIQUEIDENTIFIER | BR_UNIQUEIDENTIFIER | QT_UNIQUEIDENTIFIER) { $res = new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.UniqueIdentifier); }
        // Note: Type XML is not supported.
    ;

decimalDataType returns [DecimalDataType res]
    :   (
                DECIMAL | BR_DECIMAL | QT_DECIMAL
            |   DEC | BR_DEC | QT_DEC
            |   NUMERIC | BR_NUMERIC | QT_NUMERIC
        )
        (LPAREN i1=Integer (COMMA i2=Integer)? RPAREN)?
        {
            $res = new DecimalDataType(
                $i1 != null ? Int32.Parse($i1.Text) : DecimalDataType.PrecisionDefault,
                $i2 != null ? Int32.Parse($i2.Text) : DecimalDataType.ScaleDefault);
        }
    ;

floatDataType returns [FloatDataType res]
    :   (
                FLOAT | BR_FLOAT | QT_FLOAT
            |   DOUBLE PRECISION
        )
        (LPAREN Integer RPAREN)?
        {
            $res = new FloatDataType(
                $Integer != null ? Int32.Parse($Integer.Text) : FloatDataType.MantissaDefault);
        }
    ;

dateTimePrecisionDataType returns [DateTimePrecisionDataType res]
    :   dateTimePrecisionDataTypeType (LPAREN Integer RPAREN)?
        {
            $res = new DateTimePrecisionDataType($dateTimePrecisionDataTypeType.res,
                $Integer != null ? Int32.Parse($Integer.Text) : DateTimePrecisionDataType.PrecisionDefault);
        }
    ;

dateTimePrecisionDataTypeType returns [DateTimePrecisionDataTypeType res]
    :   (DATETIME2 | BR_DATETIME2 | QT_DATETIME2) { $res = DateTimePrecisionDataTypeType.DateTime2; }
    |   (DATETIMEOFFSET | BR_DATETIMEOFFSET | QT_DATETIMEOFFSET) { $res = DateTimePrecisionDataTypeType.DateTimeOffset; }
    |   (TIME | BR_TIME | QT_TIME) { $res = DateTimePrecisionDataTypeType.Time; }
    ;

stringDataType returns [StringWithLengthDataType res]
    :   stringDataTypeType (LPAREN Integer RPAREN)?
        {
            $res = new StringWithLengthDataType($stringDataTypeType.res,
                $Integer != null ? Int32.Parse($Integer.Text) : 0);
        }
    ;

stringDataTypeType returns [StringWithLengthDataTypeType res]
    :   (
                CHAR | BR_CHAR | QT_CHAR
            |   CHARACTER | BR_CHARACTER | QT_CHARACTER
        )
        {
            $res = StringWithLengthDataTypeType.Char;
        }
    |   (
                NCHAR | BR_NCHAR | QT_NCHAR
            |   NATIONAL (CHAR | BR_CHAR | QT_CHAR)
            |   NATIONAL (CHARACTER | BR_CHARACTER | QT_CHARACTER)
        )
        {
            $res = StringWithLengthDataTypeType.NChar;
        }
    |   (BINARY | BR_BINARY | QT_BINARY)
        {
            $res = StringWithLengthDataTypeType.Binary;
        }
    ;

variableStringDataType returns [StringWithLengthDataType res]
    :   variableStringDataTypeType (LPAREN variableStringDataTypeLength RPAREN)?
        {
            $res = new StringWithLengthDataType($variableStringDataTypeType.res, $variableStringDataTypeLength.res);
        }
    ;

variableStringDataTypeType returns [StringWithLengthDataTypeType res]
    :   (
                VARCHAR | BR_VARCHAR | QT_VARCHAR
            |   (CHAR | BR_CHAR | QT_CHAR) VARYING
            |   (CHARACTER | BR_CHARACTER | QT_CHARACTER) VARYING
        )
        {
            $res = StringWithLengthDataTypeType.VarChar;
        }
    |   (
                NVARCHAR | BR_NVARCHAR | QT_NVARCHAR
            |   NATIONAL (CHAR | BR_CHAR | QT_CHAR) VARYING
            |   NATIONAL (CHARACTER | BR_CHARACTER | QT_CHARACTER) VARYING
        )
        {
            $res = StringWithLengthDataTypeType.NVarChar;
        }
    |   (
                VARBINARY | BR_VARBINARY | QT_VARBINARY
            |   (BINARY | BR_BINARY | QT_BINARY) VARYING
        )
        {
            $res = StringWithLengthDataTypeType.VarBinary;
        }
    ;

variableStringDataTypeLength returns [int res]
    :   Integer { $res = Int32.Parse($Integer.Text); }
    |   (MAX | BR_MAX | QT_MAX) { $res = -1; }
    ;

genericDataType returns [GenericDataType res]
    :   (gti1=genericTypeIdentifier DOT)? gti2=genericTypeIdentifier
        {
            $res = new GenericDataType($gti1.res, $gti2.res);
        }
    ;

// BASIC RULES

dbObjectList returns [List<DbObject> res]
    :   do1=dbObject { $res = new List<DbObject> { $do1.res }; }
        (COMMA do2=dbObject { $res.Add($do2.res); })*
    ;

dbObject returns [DbObject res]
    :   dbObjectIdentifiers { $res = new DbObject($dbObjectIdentifiers.res); }
    ;

dbObjectIdentifiers returns [List<Identifier> res]
    :   { $res = new List<Identifier>(); }
        (
                DOT { $res.Add(new Identifier(IdentifierType.Plain, "")); }
        )*
        i1=identifier { $res.Add($i1.res); }
        (
                DOT
                (
                        DOT { $res.Add(new Identifier(IdentifierType.Plain, "")); }
                )*
                i2=identifier { $res.Add($i2.res); }
        )*
    ;

constant returns [ConstantExpression res]
    :   stringLiteral { $res = new StringConstantExpression($stringLiteral.res); }
    |   HexLiteral { $res = new HexConstantExpression($HexLiteral.Text); }
    |   DateTime { $res = new DateTimeConstantExpression($DateTime.Text); }
    |   Integer { $res = new IntegerConstantExpression(Int32.Parse($Integer.Text)); }
    |   Decimal { $res = new DecimalConstantExpression(System.Decimal.Parse($Decimal.Text)); }
    |   Real { $res = new RealConstantExpression(new RealLiteral($Real.Text)); }
    |   Money { $res = new MoneyConstantExpression(new MoneyLiteral($Money.Text)); }
    |   NULL { $res = new NullConstantExpression(); }
    ;

stringLiteral returns [StringLiteral res]
    :   UnicodeStringLiteral { $res = new StringLiteral(StringLiteralType.Unicode, $UnicodeStringLiteral.Text); }
    |   ASCIIStringLiteral { $res = new StringLiteral(StringLiteralType.ASCII, $ASCIIStringLiteral.Text); }
    ;

decimalLiteral returns [Decimal res]
    :   Integer { $res = System.Decimal.Parse($Integer.Text); }
    |   Decimal { $res = System.Decimal.Parse($Decimal.Text); }
    ;

defaultSupportingExpression returns [Expression res]
    :   expression { $res = $expression.res; }
    |   DEFAULT { $res = new DefaultExpression(); }
    ;

collation returns [Identifier res]
    :   COLLATE identifier { $res = $identifier.res; }
    ;

orderedColumnsInParens returns [List<OrderedColumn> res]
    :   LPAREN
        oc1=orderedColumn { $res = new List<OrderedColumn> { $oc1.res }; }
        (COMMA oc2=orderedColumn { $res.Add($oc2.res); })*
        RPAREN
    ;

orderedColumn returns [OrderedColumn res]
    :   identifier orderDirection? { $res = new OrderedColumn($identifier.res, $orderDirection.res); }
    ;

orderDirection returns [OrderDirection res]
    :   ASC { $res = OrderDirection.Ascending; }
    |   DESC { $res = OrderDirection.Descending; }
    ;

compoundAssignmentOperator returns [AssignmentType res]
    :   addAssign { $res = AssignmentType.AddAssign; }
    |   subAssign { $res = AssignmentType.SubAssign; }
    |   mulAssign { $res = AssignmentType.MulAssign; }
    |   divAssign { $res = AssignmentType.DivAssign; }
    |   modAssign { $res = AssignmentType.ModAssign; }
    |   bitAndAssign { $res = AssignmentType.AndAssign; }
    |   bitXorAssign { $res = AssignmentType.XorAssign; }
    |   bitOrAssign { $res = AssignmentType.OrAssign; }
    ;

assignmentOperator returns [AssignmentType res]
    :   compoundAssignmentOperator { $res = $compoundAssignmentOperator.res; }
    |   EQUAL { $res = AssignmentType.Assign; }
    ;

identifierListInParens returns [List<Identifier> res]
    :   LPAREN
        i1=identifier { $res = new List<Identifier> { $i1.res }; }
        (COMMA i2=identifier { $res.Add($i2.res); })*
        RPAREN
    ;

identifierOrIdentifiersListInParens returns [List<Identifier> res]
	: statementEndIdentifier { $res = new List<Identifier> { $statementEndIdentifier.res }; }
	| identifierListInParens { $res = $identifierListInParens.res; }
	;

// Generic identifier with no constraints.
identifier returns [Identifier res]
    :   baseIdentifier { $res = $baseIdentifier.res; }
        // All non-keywords.
    |   builtinTypeNonKeyword { $res = new Identifier(IdentifierType.Plain, $builtinTypeNonKeyword.res); }
    |   builtinFunctionNonKeyword { $res = new Identifier(IdentifierType.Plain, $builtinFunctionNonKeyword.res); }
    |   statementStartNonKeyword { $res = new Identifier(IdentifierType.Plain, $statementStartNonKeyword.res); }
    |   nonKeyword { $res = new Identifier(IdentifierType.Plain, $nonKeyword.res); }
        // All special bracketed and quoted identifiers.
    |   builtinTypeBracketedIdentifier { $res = new Identifier(IdentifierType.Bracketed, $builtinTypeBracketedIdentifier.res); }
    |   builtinTypeQuotedIdentifier { $res = new Identifier(IdentifierType.Quoted, $builtinTypeQuotedIdentifier.res); }
    ;

// Identifier that can be used for data types, limited by conflicts with built-in type names.
genericTypeIdentifier returns [Identifier res]
    :   baseIdentifier { $res = $baseIdentifier.res; }
        // Non-keywords that are not valid builtinDataType names.
    |   builtinFunctionNonKeyword { $res = new Identifier(IdentifierType.Plain, $builtinFunctionNonKeyword.res); }
    |   statementStartNonKeyword { $res = new Identifier(IdentifierType.Plain, $statementStartNonKeyword.res); }
    |   nonKeyword { $res = new Identifier(IdentifierType.Plain, $nonKeyword.res); }
    ;

// Identifier that can be used for function names, limited by conflicts with built-in function names.
genericFunctionIdentifier returns [Identifier res]
	// Schema is not remembered
    :   (bi1=baseIdentifier DOT)? bi2=baseIdentifier { $res = $bi2.res; }
        // Non-keywords that are not valid specialScalarFunction names.
    |   builtinTypeNonKeyword { $res = new Identifier(IdentifierType.Plain, $builtinTypeNonKeyword.res); }
    |   statementStartNonKeyword { $res = new Identifier(IdentifierType.Plain, $statementStartNonKeyword.res); }
    |   nonKeyword { $res = new Identifier(IdentifierType.Plain, $nonKeyword.res); }
        // Keywords that are also valid built-in functions (and handled as generic functions).
    |   COALESCE { $res = new Identifier(IdentifierType.Plain, $COALESCE.Text); }
    |   LEFT { $res = new Identifier(IdentifierType.Plain, $LEFT.Text); }
    |   NULLIF { $res = new Identifier(IdentifierType.Plain, $NULLIF.Text); }
    |   RIGHT { $res = new Identifier(IdentifierType.Plain, $RIGHT.Text); }
    |   UPDATE { $res = new Identifier(IdentifierType.Plain, $UPDATE.Text); }
        // All special bracketed and quoted identifiers.
    |   builtinTypeBracketedIdentifier { $res = new Identifier(IdentifierType.Bracketed, $builtinTypeBracketedIdentifier.res); }
    |   builtinTypeQuotedIdentifier { $res = new Identifier(IdentifierType.Quoted, $builtinTypeQuotedIdentifier.res); }
    ;

// Identifier that can be used at statement end, limited by conflicts between statement end and statement beginning.
statementEndIdentifier returns [Identifier res]
    :   baseIdentifier { $res = $baseIdentifier.res; }
        // Non-keywords that can start a new statement would cause conflicts at the end of statements, so they are excluded.
    |   builtinTypeNonKeyword { $res = new Identifier(IdentifierType.Plain, $builtinTypeNonKeyword.res); }
    |   builtinFunctionNonKeyword { $res = new Identifier(IdentifierType.Plain, $builtinFunctionNonKeyword.res); }
    |   nonKeyword { $res = new Identifier(IdentifierType.Plain, $nonKeyword.res); }
        // All special bracketed and quoted identifiers.
    |   builtinTypeBracketedIdentifier { $res = new Identifier(IdentifierType.Bracketed, $builtinTypeBracketedIdentifier.res); }
    |   builtinTypeQuotedIdentifier { $res = new Identifier(IdentifierType.Quoted, $builtinTypeQuotedIdentifier.res); }
    ;

baseIdentifier returns [Identifier res]
    :   PlainIdentifier { $res = new Identifier(IdentifierType.Plain, $PlainIdentifier.Text); }
    |   QuotedIdentifier { $res = new Identifier(IdentifierType.Quoted, $QuotedIdentifier.Text); }
    |   BracketedIdentifier { $res = new Identifier(IdentifierType.Bracketed, $BracketedIdentifier.Text); }
    ;

// All built-in type names that are not keywords.
builtinTypeNonKeyword returns [string res]
    :   BIGINT { $res = $BIGINT.Text; }
    |   BINARY { $res = $BINARY.Text; }
    |   BIT { $res = $BIT.Text; }
    |   CHAR { $res = $CHAR.Text; }
    |   CHARACTER { $res = $CHARACTER.Text; }
    |   DATE { $res = $DATE.Text; }
    |   DATETIME { $res = $DATETIME.Text; }
    |   DATETIME2 { $res = $DATETIME2.Text; }
    |   DATETIMEOFFSET { $res = $DATETIMEOFFSET.Text; }
    |   DEC { $res = $DEC.Text; }
    |   DECIMAL { $res = $DECIMAL.Text; }
    |   FLOAT { $res = $FLOAT.Text; }
    |   HIERARCHYID { $res = $HIERARCHYID.Text; }
    |   IMAGE { $res = $IMAGE.Text; }
    |   INT { $res = $INT.Text; }
    |   INTEGER { $res = $INTEGER.Text; }
    |   MONEY { $res = $MONEY.Text; }
    |   NCHAR { $res = $NCHAR.Text; }
    |   NTEXT { $res = $NTEXT.Text; }
    |   NUMERIC { $res = $NUMERIC.Text; }
    |   NVARCHAR { $res = $NVARCHAR.Text; }
    |   REAL { $res = $REAL.Text; }
    |   ROWVERSION { $res = $ROWVERSION.Text; }
    |   SMALLDATETIME { $res = $SMALLDATETIME.Text; }
    |   SMALLINT { $res = $SMALLINT.Text; }
    |   SMALLMONEY { $res = $SMALLMONEY.Text; }
    |   SQL_VARIANT { $res = $SQL_VARIANT.Text; }
    |   TEXT { $res = $TEXT.Text; }
    |   TIME { $res = $TIME.Text; }
    |   TIMESTAMP { $res = $TIMESTAMP.Text; }
    |   TINYINT { $res = $TINYINT.Text; }
    |   UNIQUEIDENTIFIER { $res = $UNIQUEIDENTIFIER.Text; }
    |   VARBINARY { $res = $VARBINARY.Text; }
    |   VARCHAR { $res = $VARCHAR.Text; }
    ;

// Bracketed identifiers that are used in built-in data types.
builtinTypeBracketedIdentifier returns [string res]
    :   BR_BIGINT { $res = $BR_BIGINT.Text; }
    |   BR_BINARY { $res = $BR_BINARY.Text; }
    |   BR_BIT { $res = $BR_BIT.Text; }
    |   BR_CHAR { $res = $BR_CHAR.Text; }
    |   BR_CHARACTER { $res = $BR_CHARACTER.Text; }
    |   BR_DATE { $res = $BR_DATE.Text; }
    |   BR_DATETIME { $res = $BR_DATETIME.Text; }
    |   BR_DATETIME2 { $res = $BR_DATETIME2.Text; }
    |   BR_DATETIMEOFFSET { $res = $BR_DATETIMEOFFSET.Text; }
    |   BR_DEC { $res = $BR_DEC.Text; }
    |   BR_DECIMAL { $res = $BR_DECIMAL.Text; }
    |   BR_FLOAT { $res = $BR_FLOAT.Text; }
    |   BR_HIERARCHYID { $res = $BR_HIERARCHYID.Text; }
    |   BR_IMAGE { $res = $BR_IMAGE.Text; }
    |   BR_INT { $res = $BR_INT.Text; }
    |   BR_INTEGER { $res = $BR_INTEGER.Text; }
    |   BR_MAX { $res = $BR_MAX.Text; }
    |   BR_MONEY { $res = $BR_MONEY.Text; }
    |   BR_NCHAR { $res = $BR_NCHAR.Text; }
    |   BR_NTEXT { $res = $BR_NTEXT.Text; }
    |   BR_NUMERIC { $res = $BR_NUMERIC.Text; }
    |   BR_NVARCHAR { $res = $BR_NVARCHAR.Text; }
    |   BR_REAL { $res = $BR_REAL.Text; }
    |   BR_ROWVERSION { $res = $BR_ROWVERSION.Text; }
    |   BR_SMALLDATETIME { $res = $BR_SMALLDATETIME.Text; }
    |   BR_SMALLINT { $res = $BR_SMALLINT.Text; }
    |   BR_SMALLMONEY { $res = $BR_SMALLMONEY.Text; }
    |   BR_SQL_VARIANT { $res = $BR_SQL_VARIANT.Text; }
    |   BR_TEXT { $res = $BR_TEXT.Text; }
    |   BR_TIME { $res = $BR_TIME.Text; }
    |   BR_TIMESTAMP { $res = $BR_TIMESTAMP.Text; }
    |   BR_TINYINT { $res = $BR_TINYINT.Text; }
    |   BR_UNIQUEIDENTIFIER { $res = $BR_UNIQUEIDENTIFIER.Text; }
    |   BR_VARBINARY { $res = $BR_VARBINARY.Text; }
    |   BR_VARCHAR { $res = $BR_VARCHAR.Text; }
    ;

// Quoted identifiers that are used in built-in data types.
builtinTypeQuotedIdentifier returns [string res]
    :   QT_BIGINT { $res = $QT_BIGINT.Text; }
    |   QT_BINARY { $res = $QT_BINARY.Text; }
    |   QT_BIT { $res = $QT_BIT.Text; }
    |   QT_CHAR { $res = $QT_CHAR.Text; }
    |   QT_CHARACTER { $res = $QT_CHARACTER.Text; }
    |   QT_DATE { $res = $QT_DATE.Text; }
    |   QT_DATETIME { $res = $QT_DATETIME.Text; }
    |   QT_DATETIME2 { $res = $QT_DATETIME2.Text; }
    |   QT_DATETIMEOFFSET { $res = $QT_DATETIMEOFFSET.Text; }
    |   QT_DEC { $res = $QT_DEC.Text; }
    |   QT_DECIMAL { $res = $QT_DECIMAL.Text; }
    |   QT_FLOAT { $res = $QT_FLOAT.Text; }
    |   QT_HIERARCHYID { $res = $QT_HIERARCHYID.Text; }
    |   QT_IMAGE { $res = $QT_IMAGE.Text; }
    |   QT_INT { $res = $QT_INT.Text; }
    |   QT_INTEGER { $res = $QT_INTEGER.Text; }
    |   QT_MAX { $res = $QT_MAX.Text; }
    |   QT_MONEY { $res = $QT_MONEY.Text; }
    |   QT_NCHAR { $res = $QT_NCHAR.Text; }
    |   QT_NTEXT { $res = $QT_NTEXT.Text; }
    |   QT_NUMERIC { $res = $QT_NUMERIC.Text; }
    |   QT_NVARCHAR { $res = $QT_NVARCHAR.Text; }
    |   QT_REAL { $res = $QT_REAL.Text; }
    |   QT_ROWVERSION { $res = $QT_ROWVERSION.Text; }
    |   QT_SMALLDATETIME { $res = $QT_SMALLDATETIME.Text; }
    |   QT_SMALLINT { $res = $QT_SMALLINT.Text; }
    |   QT_SMALLMONEY { $res = $QT_SMALLMONEY.Text; }
    |   QT_SQL_VARIANT { $res = $QT_SQL_VARIANT.Text; }
    |   QT_TEXT { $res = $QT_TEXT.Text; }
    |   QT_TIME { $res = $QT_TIME.Text; }
    |   QT_TIMESTAMP { $res = $QT_TIMESTAMP.Text; }
    |   QT_TINYINT { $res = $QT_TINYINT.Text; }
    |   QT_UNIQUEIDENTIFIER { $res = $QT_UNIQUEIDENTIFIER.Text; }
    |   QT_VARBINARY { $res = $QT_VARBINARY.Text; }
    |   QT_VARCHAR { $res = $QT_VARCHAR.Text; }
    ;

// All built-in function names that are not keywords.
builtinFunctionNonKeyword returns [string res]
    :   AVG { $res = $AVG.Text; }
    |   BINARY_CHECKSUM { $res = $BINARY_CHECKSUM.Text; }
    |   CAST { $res = $CAST.Text; }
    |   CHECKSUM { $res = $CHECKSUM.Text; }
    |   CHECKSUM_AGG { $res = $CHECKSUM_AGG.Text; }
	|	CHOOSE { $res = $CHOOSE.Text; }
    |   COUNT { $res = $COUNT.Text; }
    |   COUNT_BIG { $res = $COUNT_BIG.Text; }
    |   CUME_DIST { $res = $CUME_DIST.Text; }
    |   DATENAME { $res = $DATENAME.Text; }
    |   DATEPART { $res = $DATEPART.Text; }
    |   DENSE_RANK { $res = $DENSE_RANK.Text; }
    |   FIRST_VALUE { $res = $FIRST_VALUE.Text; }
	|	IIF { $res = $IIF.Text; }
	|   LAG { $res = $LAG.Text; }
    |   LAST_VALUE { $res = $LAST_VALUE.Text; }
    |   LEAD { $res = $LEAD.Text; }
    |   MAX { $res = $MAX.Text; }
        // MIN_ACTIVE_ROWVERSION is not a keyword according to MSDN, however it is a parameterless
        // function used without parenthesis. If it is allowed as identifier as well, the grammar 
        // would be ambiguous.
        //    |   MIN_ACTIVE_ROWVERSION { $res = $MIN_ACTIVE_ROWVERSION.Text; }
    |   MIN { $res = $MIN.Text; }
    |   NEXT { $res = $NEXT.Text; }
    |   NTILE { $res = $NTILE.Text; }
    |   PARSE { $res = $PARSE.Text; }
    |   PERCENT_RANK { $res = $PERCENT_RANK.Text; }
    |   PERCENTILE_CONT { $res = $PERCENTILE_CONT.Text; }
    |   PERCENTILE_DISC { $res = $PERCENTILE_DISC.Text; }
    |   RANK { $res = $RANK.Text; }
    |   ROW_NUMBER { $res = $ROW_NUMBER.Text; }
    |   STDEV { $res = $STDEV.Text; }
    |   STDEVP { $res = $STDEVP.Text; }
    |   SUM { $res = $SUM.Text; }
    |   VAR { $res = $VAR.Text; }
    |   VARP { $res = $VARP.Text; }
    |   WRITE { $res = $WRITE.Text; }
    |   TRY_PARSE { $res = $TRY_PARSE.Text; }
    ;

// Non-keywords that can start a new statement. They will cause conflicts if used
// at the end of statements, because parser will not be able to determine where a
// new statement ends.
statementStartNonKeyword returns [string res]
    :   GO { $res = $GO.Text; }
    |   THROW { $res = $THROW.Text; }
    |   TRY { $res = $TRY.Text; }
    ;

// All words that are not keywords (and not included in builtinTypeNonKeyword or builtinFunctionNonKeyword).
nonKeyword returns [string res]
    :   ABSOLUTE { $res = $ABSOLUTE.Text; }
    |   ACTION { $res = $ACTION.Text; }
    |   AFTER {$res = $AFTER.Text; }
    |   ALLOW_PAGE_LOCKS { $res = $ALLOW_PAGE_LOCKS.Text; }
    |   ALLOW_ROW_LOCKS { $res = $ALLOW_ROW_LOCKS.Text; }
    |	ANSI_DEFAULTS { $res = $ANSI_DEFAULTS.Text; }
	|	ANSI_PADDING { $res = $ANSI_PADDING.Text; }
	|	ANSI_NULLS { $res = $ANSI_NULLS.Text; }
	|	ANSI_NULL_DFLT_OFF { $res = $ANSI_NULL_DFLT_OFF.Text; }
	|	ANSI_NULL_DFLT_ON { $res = $ANSI_NULL_DFLT_ON.Text; }
	|   ANSI_WARNINGS { $res = $ANSI_WARNINGS.Text; }
	|	ARITHABORT { $res = $ARITHABORT.Text; }
	|	ARITHIGNORE { $res = $ARITHIGNORE.Text; }
    |   AT { $res = $AT.Text; }
    |   AUTO { $res = $AUTO.Text; }
//    |   BASE64 { $res = $BASE64.Text; }
	|	CALLED { $res = $CALLED.Text; }
	|	CALLER { $res = $CALLER.Text; }
    |   CATCH { $res = $CATCH.Text; }
    |   CHANGE_TRACKING { $res = $CHANGE_TRACKING.Text; }
    |   CODEPAGE { $res = $CODEPAGE.Text; }
	|	COLUMNS { $res = $COLUMNS.Text; }
    |   CONCAT { $res = $CONCAT.Text; }
	|	CONCAT_NULL_YIELDS_NULL { $res = $CONCAT_NULL_YIELDS_NULL.Text; }

//    |   CUBE { $res = $CUBE.Text; } // conflict between expression and groupingSpec
	|	CURSOR_CLOSE_ON_COMMIT { $res = $CURSOR_CLOSE_ON_COMMIT.Text; }
    |   DATABASE_DEFAULT { $res = $DATABASE_DEFAULT.Text; }
    |   DATA_COMPRESSION { $res = $DATA_COMPRESSION.Text; }
	|	DATEFIRST { $res = $DATEFIRST.Text; }
	|	DATEFORMAT { $res = $DATEFORMAT.Text; }
    |   DELAY { $res = $DELAY.Text; }
    |   DISABLE { $res = $DISABLE.Text; }
	|	DMY { $res = $DMY.Text; }
    |   DROP_EXISTING { $res = $DROP_EXISTING.Text; }
	|	DYM	{ $res = $DYM.Text; }
    |   DYNAMIC { $res = $DYNAMIC.Text; }
//    |   ELEMENTS { $res = $ELEMENTS.Text; }
    |   ENABLE { $res = $ENABLE.Text; }
    |   ENCRYPTION { $res = $ENCRYPTION.Text; }
    |   ERRORFILE { $res = $ERRORFILE.Text; }
    |   EXPAND { $res = $EXPAND.Text; }
//    |   EXPLICIT { $res = $EXPLICIT.Text; }
    |   FAST { $res = $FAST.Text; }
    |   FAST_FORWARD { $res = $FAST_FORWARD.Text; }
    |   FASTFIRSTROW { $res = $FASTFIRSTROW.Text; }
    |   FIRST { $res = $FIRST.Text; }
    |   FILESTREAM { $res = $FILESTREAM.Text; }
    |   FILESTREAM_ON { $res = $FILESTREAM_ON.Text; }
    |   FILETABLE { $res = $FILETABLE.Text; }
    |   FILETABLE_COLLATE_FILENAME { $res = $FILETABLE_COLLATE_FILENAME.Text; }
    |   FILETABLE_DIRECTORY { $res = $FILETABLE_DIRECTORY.Text; }
    |   FILETABLE_NAMESPACE { $res = $FILETABLE_NAMESPACE.Text; }
    |   FILETABLE_FULLPATH_UNIQUE_CONSTRAINT_NAME { $res = $FILETABLE_FULLPATH_UNIQUE_CONSTRAINT_NAME.Text; }
    |   FILETABLE_PRIMARY_KEY_CONSTRAINT_NAME { $res = $FILETABLE_PRIMARY_KEY_CONSTRAINT_NAME.Text; }
    |   FILETABLE_STREAMID_UNIQUE_CONSTRAINT_NAME { $res = $FILETABLE_STREAMID_UNIQUE_CONSTRAINT_NAME.Text; }
    |   FIRSTROW { $res = $FIRSTROW.Text; }
	|	FMTONLY { $res = $FMTONLY.Text; }
    |   FOLLOWING { $res = $FOLLOWING.Text; }
    |   FORCE { $res = $FORCE.Text; }
    |   FORCED { $res = $FORCED.Text; }
	|	FORCEPLAN { $res = $FORCEPLAN.Text; }
    |   FORCESCAN { $res = $FORCESCAN.Text; }
    |   FORCESEEK { $res = $FORCESEEK.Text; }
    |   FORMATFILE { $res = $FORMATFILE.Text; }
    |   FORWARD_ONLY { $res = $FORWARD_ONLY.Text; }
	|	FULLSCAN { $res = $FULLSCAN.Text; }
    |   GROUPING { $res = $GROUPING.Text; }
    |   GLOBAL { $res = $GLOBAL.Text; }
    |   HASH { $res = $HASH.Text; }
    |   HINT { $res = $HINT.Text; }
	|	IDENTITY_INSERT { $res = $IDENTITY_INSERT.text; }
    |   IGNORE_DUP_KEY { $res = $IGNORE_DUP_KEY.Text; }
    |   IGNORE_NONCLUSTERED_COLUMNSTORE_INDEX { $res = $IGNORE_NONCLUSTERED_COLUMNSTORE_INDEX.Text; }
	|	IMPLICIT_TRANSACTIONS { $res = $IMPLICIT_TRANSACTIONS.Text; }
    |   INCLUDE { $res = $INCLUDE.Text; }
	|	INPUT { $res = $INPUT.Text; }
    |   INSTEAD { $res = $INSTEAD.Text; }
	|	IO { $res = $IO.Text; }
    |   KEEP { $res = $KEEP.Text; }
    |   KEYSET { $res = $KEYSET.Text; }
    |   KEEPFIXED { $res = $KEEPFIXED.Text; }
    |   LANGUAGE { $res = $LANGUAGE.Text; }
    |   LAST { $res = $LAST.Text; }
    |   LASTROW { $res = $LASTROW.Text; }
    |   LOB_COMPACTION { $res = $LOB_COMPACTION.Text; }
    |   LOCAL { $res = $LOCAL.Text; }
    |   LOCK_ESCALATION { $res = $LOCK_ESCALATION.Text; }
	|	LOCK_TIMEOUT { $res = $LOCK_TIMEOUT.Text; }
    |   LOOP { $res = $LOOP.Text; }
    |   MARK { $res = $MARK.Text; }
    |   MAXDOP { $res = $MAXDOP.Text; }
    |   MAXERRORS { $res = $MAXERRORS.Text; }
    |   MAXRECURSION { $res = $MAXRECURSION.Text; }
	|	MDY	{ $res = $MDY.Text; }
    |   MOVE { $res = $MOVE.Text; }
	|	MYD	{ $res = $MYD.Text; }
    |   NAME { $res = $NAME.Text; }
    |   NO { $res = $NO.Text; }
	|	NOCOUNT { $res = $NOCOUNT.Text; }
	|	NOEXEC { $res = $NOEXEC.Text; }
    |   NOEXPAND { $res = $NOEXPAND.Text; }
    |   NOLOCK { $res = $NOLOCK.Text; }
    |   NONE { $res = $NONE.Text; }
	|	NORECOMPUTE { $res = $NORECOMPUTE.Text; }
    |   NOWAIT { $res = $NOWAIT.Text; }
	|	NUMERIC_ROUNDABORT { $res = $NUMERIC_ROUNDABORT.Text; }
    |   OBJECT { $res = $OBJECT.Text; }
    |   ONLINE { $res = $ONLINE.Text; }
    |   OPTIMIZE { $res = $OPTIMIZE.Text; }
    |   OPTIMISTIC { $res = $OPTIMISTIC.Text; }
    |   OWNER { $res = $OWNER.Text; }
    |   PAD_INDEX { $res = $PAD_INDEX.Text; }
    |   PAGE { $res = $PAGE.Text; }
    |   PAGLOCK { $res = $PAGLOCK.Text; }
    |   PARAMETERIZATION { $res = $PARAMETERIZATION.Text; }
	|	PARSEONLY { $res = $PARSEONLY.Text; }
    |   PARTITION { $res = $PARTITION.Text; }
    |   PARTITIONS { $res = $PARTITIONS.Text; }
    |   PERSISTED { $res = $PERSISTED.Text; }
    |   PRECEDING { $res = $PRECEDING.Text; }
    |   PRIOR { $res = $PRIOR.Text; }
	|	PROFILE { $res = $PROFILE.Text; }
	|	QUOTED_IDENTIFIER { $res = $QUOTED_IDENTIFIER.Text; }
    |   RANGE { $res = $RANGE.Text; }
//    |   RAW { $res = $RAW.Text; }
    |   RECOMPILE { $res = $RECOMPILE.Text; }
    |   READCOMMITTED { $res = $READCOMMITTED.Text; }
    |   READCOMMITTEDLOCK { $res = $READCOMMITTEDLOCK.Text; }
    |   READONLY { $res = $READONLY.Text; }
    |   READ_ONLY { $res = $READ_ONLY.Text; }
    |   READPAST { $res = $READPAST.Text; }
    |   READUNCOMMITTED { $res = $READUNCOMMITTED.Text; }
    |   REBUILD { $res = $REBUILD.Text; }
    |   RELATIVE { $res = $RELATIVE.Text; }
    |   REMOTE { $res = $REMOTE.Text; }
	|	REMOTE_PROC_TRANSACTIONS { $res = $REMOTE_PROC_TRANSACTIONS.Text; }
    |   REORGANIZE { $res = $REORGANIZE.Text; }
    |   REPEATABLE { $res = $REPEATABLE.Text; }
    |   REPEATABLEREAD { $res = $REPEATABLEREAD.Text; }
	|	RESAMPLE { $res = $RESAMPLE.Text; }
    |   RESULT { $res = $RESULT.Text; }
	|	RETURNS { $res = $RETURNS.Text; }
    |   ROBUST { $res = $ROBUST.Text; }
//    |   ROLLUP { $res = $ROLLUP.Text; } // conflict between expression and groupingSpec
    |   ROW { $res = $ROW.Text; }
    |   ROWLOCK { $res = $ROWLOCK.Text; }
    |   ROWS { $res = $ROWS.Text; }
    |   ROWS_PER_BATCH { $res = $ROWS_PER_BATCH.Text; }
	|	SAMPLE { $res = $SAMPLE.Text; }
    |   SCHEMABINDING { $res = $SCHEMABINDING.Text; }
    |   SCROLL { $res = $SCROLL.Text; }
    |   SCROLL_LOCKS { $res = $SCROLL_LOCKS.Text; }
    |   SELF { $res = $SELF.Text; }
    |   SERIALIZABLE { $res = $SERIALIZABLE.Text; }
    |   SETS { $res = $SETS.Text; }
	|	SHOWPLAN_ALL { $res = $SHOWPLAN_ALL.Text; }
	|	SHOWPLAN_TEXT { $res = $SHOWPLAN_TEXT.Text; }
	|	SHOWPLAN_XML { $res = $SHOWPLAN_XML.Text; }
    |   SIMPLE { $res = $SIMPLE.Text; }
    |   SINGLE_BLOB { $res = $SINGLE_BLOB.Text; }
    |   SINGLE_CLOB { $res = $SINGLE_CLOB.Text; }
    |   SINGLE_NCLOB { $res = $SINGLE_NCLOB.Text; }
    |   SORT_IN_TEMPDB { $res = $SORT_IN_TEMPDB.Text; }
    |   SPARSE { $res = $SPARSE.Text; }
    |   SPATIAL_WINDOW_MAX_CELLS { $res = $SPATIAL_WINDOW_MAX_CELLS.Text; }
    |   STATIC { $res = $STATIC.Text; }
    |   STATISTICS_NORECOMPUTE { $res = $STATISTICS_NORECOMPUTE.Text; }
    |   SWITCH { $res = $SWITCH.Text; }
    |   SYSTEM { $res = $SYSTEM.Text; }
    |   TABLOCK { $res = $TABLOCK.Text; }
    |   TABLOCKX { $res = $TABLOCKX.Text; }
    |   TEXTIMAGE_ON { $res = $TEXTIMAGE_ON.Text; }
    |   TIES { $res = $TIES.Text; }
    |   TRACK_COLUMNS_UPDATED { $res = $TRACK_COLUMNS_UPDATED.Text; }
    |   TYPE { $res = $TYPE.Text; }
    |   TYPE_WARNING { $res = $TYPE_WARNING.Text; }
    |   UNBOUNDED { $res = $UNBOUNDED.Text; }
    |   UNDEFINED { $res = $UNDEFINED.Text; }
    |   UNKNOWN { $res = $UNKNOWN.Text; }
    |   UPDLOCK { $res = $UPDLOCK.Text; }
    |   USING { $res = $USING.Text; }
    |   VALUE { $res = $VALUE.Text; }
    |   VIEW_METADATA { $res = $VIEW_METADATA.Text; }
    |   VIEWS { $res = $VIEWS.Text; }
    |   WITHIN { $res = $WITHIN.Text; }
    |   WORK { $res = $WORK.Text; }
	|	XACT_ABORT { $res = $XACT_ABORT.Text; }
    |   XLOCK { $res = $XLOCK.Text; }
    |   XML { $res = $XML.Text; }
//    |   XMLDATA { $res = $XMLDATA.Text; }
	|	YDM	{ $res = $YDM.Text; }
	|	YMD	{ $res = $YMD.Text; }
    ;

// Two character operators support space between the characters, so they need to be modeled in parser.
notEqual
    :   LESSTHAN GREATERTHAN
    |   EXCLAMATION EQUAL
    ;

lessThanOrEqual
    :   LESSTHAN EQUAL
    |   EXCLAMATION GREATERTHAN
    ;

greaterThanOrEqual
    :   GREATERTHAN EQUAL
    |   EXCLAMATION LESSTHAN
    ;

addAssign
    :   PLUS EQUAL
    ;

subAssign
    :   MINUS EQUAL
    ;

mulAssign
    :   ASTERISK EQUAL
    ;

divAssign
    :   DIVIDE EQUAL
    ;

modAssign
    :   MODULO EQUAL
    ;

bitXorAssign
    :   CHEVRON EQUAL
    ;

bitAndAssign
    :   AMPERSAND EQUAL
    ;

bitOrAssign
    :   PIPE EQUAL
    ;

// LEXER

// Keywords
// (reserved words that are not valid as identifiers)

ADD : 'add' ;
ALL : 'all' ;
ALTER : 'alter' ;
AND : 'and' ;
ANY : 'any' ;
AS : 'as' ;
ASC : 'asc' ;
//AUTHORIZATION : 'authorization' ;
//BACKUP : 'backup' ;
BEGIN : 'begin' ;
BETWEEN : 'between' ;
BREAK : 'break' ;
BROWSE : 'browse' ;
BULK : 'bulk' ;
BY : 'by' ;
CASCADE : 'cascade' ;
CASE : 'case' ;
CHECK : 'check' ;
//CHECKPOINT : 'checkpoint' ;
CLOSE : 'close' ;
CLUSTERED : 'clustered' ;
COALESCE : 'coalesce' ;
COLLATE : 'collate' ;
COLUMN : 'column' ;
COMMIT : 'commit' ;
COMPUTE : 'compute' ;
CONSTRAINT : 'constraint' ;
CONTAINS : 'contains' ;
//CONTAINSTABLE : 'containstable' ;
CONTINUE : 'continue' ;
CONVERT : 'convert' ;
CREATE : 'create' ;
CROSS : 'cross' ;
CURRENT : 'current' ;
//CURRENT_DATE : 'current_date' ;
//CURRENT_TIME : 'current_time' ;
CURRENT_TIMESTAMP : 'current_timestamp' ;
CURRENT_USER : 'current_user' ;
CURSOR : 'cursor' ;
//DATABASE : 'database' ;
//DBCC : 'dbcc' ;
DEALLOCATE : 'deallocate' ;
DECLARE : 'declare' ;
DEFAULT : 'default' ;
DELETE : 'delete' ;
//DENY : 'deny' ;
DESC : 'desc' ;
//DISK : 'disk' ;
DISTINCT : 'distinct' ;
//DISTRIBUTED : 'distributed' ;
DOUBLE : 'double' ;
DROP : 'drop' ;
//DUMP : 'dump' ;
ELSE : 'else' ;
END : 'end' ;
//ERRLVL : 'errlvl' ;
ESCAPE : 'escape' ;
EXCEPT : 'except' ;
EXEC : 'exec' ;
EXECUTE : 'execute' ;
EXISTS : 'exists' ;
//EXIT : 'exit' ;
EXTERNAL : 'external' ;
FETCH : 'fetch' ;
//FILE : 'file' ;
FILLFACTOR : 'fillfactor' ;
FOR : 'for' ;
FOREIGN : 'foreign' ;
FREETEXT : 'freetext' ;
//FREETEXTTABLE : 'freetexttable' ;
FROM : 'from' ;
FULL : 'full' ;
FUNCTION : 'function' ;
GOTO : 'goto' ;
//GRANT : 'grant' ;
GROUP : 'group' ;
HAVING : 'having' ;
HOLDLOCK : 'holdlock' ;
IDENTITY : 'identity' ;
//IDENTITYCOL : 'identitycol' ;
//IDENTITY_INSERT : 'identity_insert' ;
IF : 'if' ;
IN : 'in' ;
INDEX : 'index' ;
INNER : 'inner' ;
INSERT : 'insert' ;
INTERSECT : 'intersect' ;
INTO : 'into' ;
IS : 'is' ;
JOIN : 'join' ;
KEY : 'key' ;
//KILL : 'kill' ;
LEFT : 'left' ;
LIKE : 'like' ;
//LINENO : 'lineno' ;
//LOAD : 'load' ;
MERGE : 'merge' ;
NATIONAL : 'national' ;
NOCHECK : 'nocheck' ;
NONCLUSTERED : 'nonclustered' ;
NOT : 'not' ;
NULL : 'null' ;
NULLIF : 'nullif' ;
OF : 'of' ;
OFF : 'off' ;
//OFFSETS : 'offsets' ;
ON : 'on' ;
OPEN : 'open' ;
OPENDATASOURCE : 'opendatasource' ;
OPENQUERY : 'openquery' ;
OPENROWSET : 'openrowset' ;
OPENXML : 'openxml' ;
OPTION : 'option' ;
OR : 'or' ;
ORDER : 'order' ;
OUTER : 'outer' ;
OVER : 'over' ;
PERCENT : 'percent' ;
//PIVOT : 'pivot' ;
PLAN : 'plan' ;
PRECISION : 'precision' ;
PRIMARY : 'primary' ;
PRINT : 'print' ;
PROC : 'proc' ;
PROCEDURE : 'procedure' ;
//PUBLIC : 'public' ;
RAISERROR : 'raiserror' ;
//READ : 'read' ;
//READTEXT : 'readtext' ;
//RECONFIGURE : 'reconfigure' ;
REFERENCES : 'references' ;
REPLICATION : 'replication' ;
//RESTORE : 'restore' ;
//RESTRICT : 'restrict' ;
RETURN : 'return' ;
//REVERT : 'revert' ;
//REVOKE : 'revoke' ;
RIGHT : 'right' ;
ROLLBACK : 'rollback' ;
//ROWCOUNT : 'rowcount' ;
ROWGUIDCOL : 'rowguidcol' ;
//RULE : 'rule' ;
SAVE : 'save' ;
//SCHEMA : 'schema' ;
//SECURITYAUDIT : 'securityaudit' ;
SELECT : 'select' ;
//SEMANTICKEYPHRASETABLE : 'semantickeyphrasetable' ;
//SEMANTICSIMILARITYDETAILSTABLE : 'semanticsimilaritydetailstable' ;
//SEMANTICSIMILARITYTABLE : 'semanticsimilaritytable' ;
SESSION_USER : 'session_user' ;
SET : 'set' ;
//SETUSER : 'setuser' ;
//SHUTDOWN : 'shutdown' ;
SOME : 'some' ;
STATISTICS : 'statistics' ;
SYSTEM_USER : 'system_user' ;
TABLE : 'table' ;
TABLESAMPLE : 'tablesample' ;
//TEXTSIZE : 'textsize' ;
THEN : 'then' ;
TO : 'to' ;
TOP : 'top' ;
TRAN : 'tran' ;
TRANSACTION : 'transaction' ;
TRIGGER : 'trigger' ;
TRUNCATE : 'truncate' ;
TRY_CONVERT : 'try_convert' ;
//TSEQUAL : 'tsequal' ;
UNION : 'union' ;
UNIQUE : 'unique' ;
//UNPIVOT : 'unpivot' ;
UPDATE : 'update' ;
//UPDATETEXT : 'updatetext' ;
USE : 'use' ;
USER : 'user' ;
VALUES : 'values' ;
VARYING : 'varying' ;
VIEW : 'view' ;
WAITFOR : 'waitfor' ;
WHEN : 'when' ;
WHERE : 'where' ;
WHILE : 'while' ;
WITH : 'with' ;
//WITHINGROUP : 'withingroup' ;
//WRITETEXT : 'writetext' ;

// Non-keywords
// (other words used in grammar that are also valid as identifiers)

ABSOLUTE : 'absolute' ;
ACTION : 'action' ;
AFTER : 'after' ;	
ALLOW_PAGE_LOCKS : 'allow_page_locks' ;
ALLOW_ROW_LOCKS : 'allow_row_locks' ;
ANSI_DEFAULTS : 'ansi_defaults' ;
ANSI_NULLS : 'ansi_nulls' ;
ANSI_NULL_DFLT_OFF : 'ansi_null_dflt_off' ;
ANSI_NULL_DFLT_ON : 'ansi_null_dflt_on' ;
ANSI_PADDING : 'ansi_padding' ;
ANSI_WARNINGS : 'ansi_warnings' ;
ARITHABORT : 'arithabort' ;
ARITHIGNORE : 'arithignore' ;
AT : 'at' ;
AUTO : 'auto' ;
AVG : 'avg' ;
//BASE64 : 'base64' ;
BIGINT : 'bigint' ;
BINARY : 'binary' ;
BINARY_CHECKSUM : 'binary_checksum' ;
BIT : 'bit' ;
CALLED : 'called' ;
CALLER : 'caller' ;
CAST : 'cast' ;
CATCH : 'catch' ;
CHANGE_TRACKING : 'change_tracking' ;
CHAR : 'char' ;
CHARACTER : 'character' ;
CHECKSUM : 'checksum' ;
CHECKSUM_AGG : 'checksum_agg' ;
CHOOSE : 'choose' ;
CODEPAGE : 'codepage' ;
COLUMNS : 'columns' ;
CONCAT : 'concat' ;
CONCAT_NULL_YIELDS_NULL : 'concat_null_yields_null' ;
COUNT : 'count' ;
COUNT_BIG : 'count_big' ;
CUBE : 'cube' ;
CUME_DIST : 'cume_dist' ;
CURSOR_CLOSE_ON_COMMIT : 'cursor_close_on_commit' ;
DATA_COMPRESSION : 'data_compression' ;
DATABASE_DEFAULT : 'database_default' ;
DATE : 'date' ;
DATEFIRST : 'datefirst' ;
DATEFORMAT : 'dateformat' ;
DATENAME : 'datename' ;
DATEPART : 'datepart' ;
DATETIME : 'datetime' ;
DATETIME2 : 'datetime2' ;
DATETIMEOFFSET : 'datetimeoffset' ;
DEC : 'dec' ;
DECIMAL : 'decimal' ;
DELAY : 'delay' ;
DENSE_RANK : 'dense_rank' ;
DISABLE : 'disable' ;
DOLLAR_ACTION : '$action' ;
DOLLAR_IDENTITY : '$identity' ;
DOLLAR_ROWGUID : '$rowguid' ;
DROP_EXISTING : 'drop_existing' ;
DYNAMIC : 'dymanic' ;
//ELEMENTS : 'elements' ;
ENABLE : 'enable' ;
ENCRYPTION : 'encryption' ;
ERRORFILE : 'errorfile' ;
EXPAND : 'expand' ;
//EXPLICIT : 'explicit' ;
FAST : 'fast' ;
FAST_FORWARD :  'fast_forward' ;
FASTFIRSTROW : 'fastfirstrow' ;
FILESTREAM : 'filestream' ;
FILESTREAM_ON : 'filestream_on' ;
FILETABLE : 'filetable' ;
FILETABLE_COLLATE_FILENAME : 'filetable_collate_filename' ;
FILETABLE_DIRECTORY : 'filetable_directory' ;
FILETABLE_FULLPATH_UNIQUE_CONSTRAINT_NAME : 'filetable_fullpath_unique_constraint_name' ;
FILETABLE_NAMESPACE : 'filetable_namespace' ;
FILETABLE_PRIMARY_KEY_CONSTRAINT_NAME : 'filetable_primary_key_constraint_name' ;
FILETABLE_STREAMID_UNIQUE_CONSTRAINT_NAME : 'filetable_streamid_unique_constraint_name' ;
FIRST : 'first' ;
FIRST_VALUE : 'first_value' ;
FIRSTROW : 'firstrow' ;
FLOAT : 'float' ;
FMTONLY : 'fmtonly' ;
FOLLOWING : 'following' ;
FORCE : 'force' ;
FORCED : 'forced' ;
FORCEPLAN : 'forceplan' ;
FORCESCAN : 'forcescan' ;
FORCESEEK : 'forceseek' ;
FORMATFILE : 'formatfile' ;
FORWARD_ONLY : 'forward_only' ;
FULLSCAN : 'fullscan' ;
GLOBAL  :  'global' ; 
GO : 'go' ;
GROUPING : 'grouping' ;
HASH : 'hash' ;
HIERARCHYID : 'hierarchyid' ;
HINT : 'hint' ;
IDENTITY_INSERT : 'identity_insert' ;
IGNORE_DUP_KEY : 'ignore_dup_key' ;
IGNORE_NONCLUSTERED_COLUMNSTORE_INDEX : 'ignore_nonclustered_columnstore_index' ;
IIF : 'iif' ;
IMAGE : 'image' ;
IMPLICIT_TRANSACTIONS : 'implicit_transactions' ;
INCLUDE : 'include' ;
INPUT : 'input';
INSTEAD : 'instead' ;	
INT : 'int' ;
INTEGER : 'integer' ;
IO : 'io' ;
KEEP : 'keep' ;
KEEPFIXED : 'keepfixed' ;
KEYSET : 'keyset' ;
LAG : 'lag' ;
LANGUAGE : 'language' ;
LAST : 'last' ;
LAST_VALUE : 'last_value' ;
LASTROW : 'lastrow' ;
LEAD : 'lead' ;
LOB_COMPACTION : 'lob_compaction' ;
LOCAL : 'local' ;
LOCK_ESCALATION : 'lock_escalation' ;
LOCK_TIMEOUT : 'lock_timeout' ;
LOGIN : 'login' ;
LOOP : 'loop' ;
MARK : 'mark' ;
MAX : 'max' ;
MAXDOP : 'maxdop' ;
MAXERRORS : 'maxerrors' ;
MAXRECURSION : 'maxrecursion' ;
MIN : 'min' ;
MIN_ACTIVE_ROWVERSION : 'min_active_rowversion' ;
MONEY : 'money' ;
MOVE : 'move' ;
NAME : 'name' ;	
NCHAR : 'nchar' ;
NEXT : 'next' ;
NO : 'no' ;
NOCOUNT : 'nocount' ;
NOEXEC : 'noexec' ;
NOEXPAND : 'noexpand' ;
NOLOCK : 'nolock' ;
NONE : 'none' ;
NORECOMPUTE : 'norecompute';
NOWAIT : 'nowait' ;
NTEXT : 'ntext' ;
NTILE : 'ntile' ;
NUMERIC : 'numeric' ;
NUMERIC_ROUNDABORT : 'numeric_roundabort' ;
NVARCHAR : 'nvarchar' ;
OBJECT : 'object' ;
ONLINE : 'online' ;
OPTIMISTIC : 'optimistic' ;
OPTIMIZE : 'optimize' ;
OUT : 'out' ;
OUTPUT : 'output';
OWNER : 'owner' ;
PAD_INDEX : 'pad_index' ;
PAGE : 'page' ;
PAGLOCK : 'paglock' ;
PARAMETERIZATION : 'parameterization' ;
PARSE : 'parse' ;
PARSEONLY : 'parseonly' ;
PARTITION : 'partition' ;
PARTITIONS : 'partitions' ;
PERCENT_RANK : 'percent_rank' ;
PERCENTILE_CONT : 'percentile_cont' ;
PERCENTILE_DISC : 'percentile_disc' ;
PERSISTED : 'persisted' ;
PRECEDING : 'preceding' ;
PRIOR : 'prior' ;
PROFILE : 'profile' ;
QUOTED_IDENTIFIER : 'quoted_identifier' ;
RANGE : 'range' ;
RANK : 'rank' ;
//RAW : 'raw' ;
READCOMMITTED : 'readcommitted' ;
READCOMMITTEDLOCK : 'readcommittedlock' ;
READONLY : 'readonly' ;
READ_ONLY : 'read_only' ;
READPAST : 'readpast' ;
READUNCOMMITTED : 'readuncommitted' ;
REAL : 'real' ;
REBUILD : 'rebuild' ;
RECOMPILE : 'recompile' ;
RELATIVE : 'relative' ;
REMOTE : 'remote' ;
REMOTE_PROC_TRANSACTIONS : 'remote_proc_transactions' ;
REORGANIZE : 'reorganize' ;
REPEATABLE : 'repeatable' ;
REPEATABLEREAD : 'repeatableread' ;
RESAMPLE : 'resample' ;
RESULT : 'result' ;
RETURNS : 'returns' ;
ROBUST : 'robust' ;
ROLLUP : 'rollup' ;
ROW : 'row' ;
ROW_NUMBER : 'row_number' ;
ROWLOCK : 'rowlock' ;
ROWVERSION : 'rowversion' ;
ROWS : 'rows' ;
ROWS_PER_BATCH : 'rows_per_batch' ;
SAMPLE : 'sample' ;
SCHEMABINDING : 'schemabinding' ;
SCROLL  : 'scroll' ;
SCROLL_LOCKS : 'scroll_locks' ;
SERIALIZABLE : 'serializable' ;
SHOWPLAN_ALL : 'showplan_all' ;
SHOWPLAN_TEXT : 'showplan_text' ;
SHOWPLAN_XML : 'showplan_xml' ;
SIMPLE : 'simple' ;
SELF : 'self' ;
SETS : 'sets' ;
SINGLE_BLOB : 'single_blob' ;
SINGLE_CLOB : 'single_clob' ;
SINGLE_NCLOB : 'single_nclob' ;
SMALLDATETIME : 'smalldatetime' ;
SMALLINT : 'smallint' ;
SMALLMONEY : 'smallmoney' ;
SORT_IN_TEMPDB : 'sort_in_tempdb' ;
SPARSE : 'sparse' ;
SPATIAL_WINDOW_MAX_CELLS : 'spatial_window_max_cells' ;
SQL_VARIANT : 'sql_variant' ;
STATIC : 'static' ;
STATISTICS_NORECOMPUTE : 'statistics_norecompute' ;
STDEV : 'stdev' ;
STDEVP : 'stdevp' ;
SUM : 'sum' ;
SWITCH : 'switch' ;
SYSTEM : 'system' ;
TABLOCK : 'tablock' ;
TABLOCKX : 'tablockx' ;
TEXT : 'text' ;
TEXTIMAGE_ON : 'textimage_on' ;
THROW : 'throw' ;
TIES : 'ties' ;
TIME : 'time' ;
TIMESTAMP : 'timestamp' ;
TINYINT : 'tinyint' ;
TRACK_COLUMNS_UPDATED : 'track_columns_updated' ;
TRY : 'try' ;
TRY_PARSE : 'try_parse' ;
TYPE : 'type' ;
TYPE_WARNING : 'type_warning' ;
UNBOUNDED : 'unbounded' ;
UNDEFINED : 'undefined' ;
UNIQUEIDENTIFIER : 'uniqueidentifier' ;
UNKNOWN : 'unknown' ;
UPDLOCK : 'updlock' ;
USING : 'using' ;
VALUE : 'value' ;
VAR : 'var' ;
VARBINARY : 'varbinary' ;
VARCHAR : 'varchar' ;
VARP : 'varp' ;
VIEW_METADATA : 'view_metadata' ;
VIEWS : 'views' ;
WITHIN : 'within' ;
WORK : 'work' ;
WRITE : 'write' ;
XLOCK : 'xlock' ;
XML : 'xml' ;
XACT_ABORT : 'xact_abort' ;
//XMLDATA : 'xmldata' ;

// Configuration Functions
F_DATEFIRST : '@@datefirst' ;
F_DBTS : '@@dbts' ;
F_LANGID : '@@langid' ;
F_LANGUAGE : '@@language' ;
F_LOCK_TIMEOUT : '@@lock_timeout' ;
F_MAX_CONNECTIONS : '@@max_connections' ;
F_MAX_PRECISION : '@@max_precision' ;
F_NESTLEVEL : '@@nestlevel' ;
F_OPTIONS : '@@options' ;
F_REMSERVER : '@@remserver' ;
F_SERVERNAME : '@@servername' ;
F_SERVICENAME : '@@servicename' ;
F_SPID : '@@spid' ;
F_TEXTSIZE : '@@textsize' ;
F_VERSION : '@@version' ;
// Cursor Functions
F_CURSOR_ROWS : '@@cursor_rows' ;
F_FETCH_STATUS : '@@fetch_status' ;
// Metadata Functions
F_PROCID : '@@procid' ;
// System Functions
F_ERROR : '@@error' ;
F_IDENTITY : '@@identity' ;
F_ROWCOUNT : '@@rowcount' ;
F_TRANCOUNT : '@@trancount' ;
// System Statistical Functions
F_CONNECTIONS : '@@connections' ;
F_CPU_BUSY : '@@cpu_busy' ;
F_IDLE : '@@idle' ;
F_IO_BUSY : '@@io_busy' ;
F_PACKET_ERRORS : '@@packet_errors' ;
F_PACK_RECEIVED : '@@pack_received' ;
F_PACK_SENT : '@@pack_sent' ;
F_TIMETICKS : '@@timeticks' ;
F_TOTAL_ERRORS : '@@total_errors' ;
F_TOTAL_READ : '@@total_read' ;
F_TOTAL_WRITE : '@@total_write' ;

// Bracketed and quoted identifiers that are used in built-in data types.
BR_BIGINT : '[bigint]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_BINARY : '[binary]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_BIT : '[bit]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_CHAR : '[char]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_CHARACTER : '[character]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_DATE : '[date]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_DATETIME : '[datetime]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_DATETIME2 : '[datetime2]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_DATETIMEOFFSET : '[datetimeoffset]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_DEC : '[dec]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_DECIMAL : '[decimal]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_FLOAT : '[float]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_HIERARCHYID : '[hierarchyid]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_IMAGE : '[image]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_INT : '[int]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_INTEGER : '[integer]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_MAX : '[max]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_MONEY : '[money]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_NCHAR : '[nchar]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_NTEXT : '[ntext]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_NUMERIC : '[numeric]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_NVARCHAR : '[nvarchar]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_REAL : '[real]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_ROWVERSION : '[rowversion]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_SMALLDATETIME : '[smalldatetime]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_SMALLINT : '[smallint]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_SMALLMONEY : '[smallmoney]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_SQL_VARIANT : '[sql_variant]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_TEXT : '[text]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_TIME : '[time]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_TIMESTAMP : '[timestamp]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_TINYINT : '[tinyint]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_UNIQUEIDENTIFIER : '[uniqueidentifier]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_VARBINARY : '[varbinary]' { $text = $text.Substring(1, $text.Length - 2); } ;
BR_VARCHAR : '[varchar]' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_BIGINT : '"bigint"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_BINARY : '"binary"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_BIT : '"bit"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_CHAR : '"char"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_CHARACTER : '"character"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_DATE : '"date"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_DATETIME : '"datetime"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_DATETIME2 : '"datetime2"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_DATETIMEOFFSET : '"datetimeoffset"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_DEC : '"dec"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_DECIMAL : '"decimal"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_FLOAT : '"float"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_HIERARCHYID : '"hierarchyid"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_IMAGE : '"image"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_INT : '"int"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_INTEGER : '"integer"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_MAX : '"max"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_MONEY : '"money"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_NCHAR : '"nchar"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_NTEXT : '"ntext"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_NUMERIC : '"numeric"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_NVARCHAR : '"nvarchar"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_REAL : '"real"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_ROWVERSION : '"rowversion"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_SMALLDATETIME : '"smalldatetime"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_SMALLINT : '"smallint"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_SMALLMONEY : '"smallmoney"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_SQL_VARIANT : '"sql_variant"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_TEXT : '"text"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_TIME : '"time"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_TIMESTAMP : '"timestamp"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_TINYINT : '"tinyint"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_UNIQUEIDENTIFIER : '"uniqueidentifier"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_VARBINARY : '"varbinary"' { $text = $text.Substring(1, $text.Length - 2); } ;
QT_VARCHAR : '"varchar"' { $text = $text.Substring(1, $text.Length - 2); } ;

// DateFormat types

DMY : 'dmy' ;
MDY	: 'mdy' ;
YMD	: 'ymd' ;
YDM	: 'ydm' ;
MYD	: 'myd' ;
DYM	: 'dym' ;

// Operators

fragment DOT:; // generated as a part of Number rule
COLON : ':' ;
COMMA : ',' ;
SEMICOLON : ';' ;

LPAREN : '(' ;
RPAREN : ')' ;

EQUAL : '=' ;
LESSTHAN : '<' ;
GREATERTHAN : '>' ;

DIVIDE : '/' ;
PLUS : '+' ;
MINUS : '-' ;
ASTERISK : '*' ;
MODULO : '%' ;

AMPERSAND : '&' ;
TILDE : '~' ;
CHEVRON : '^' ;
PIPE : '|' ;

EXCLAMATION : '!' ;

// New lines are also handled as comments, in order to be able to tell when a comment
// starts on a new line.
NewLine
    :   ('\n' | '\r')
        {
            $channel = CommentedTokenStream.TokenChannels.Comment;
        }
    ;

Space
    :   (' ' | '\t')
        {
            Skip();
        }
    ;

// COMMENTS
SingleLineComment
    :   '--' (~('\n' | '\r'))*
        {
            $channel = CommentedTokenStream.TokenChannels.Comment;
        }
    ;

MultiLineComment
    :   '/*' (~'*')* '*' ('*' | (~('*' | '/') (~'*')* '*'))* '/'
        {
            $channel = CommentedTokenStream.TokenChannels.Comment;
        }
    ;

// LITERALS

fragment Letter
    :   'a'..'z' | '_' | '#' | '@' | '\u0080'..'\ufffe'
    ;

fragment Digit
    :   '0'..'9'
    ;

fragment Integer :;

fragment Decimal :;

fragment Real :;

fragment HexLiteral :;

fragment Money :;

fragment Currency
        // According to http://msdn.microsoft.com/en-us/library/ms188688%28v=sql.105%29.aspx
    :   '\u0024' | '\u00a2'..'\u00a5' | '\u09f2' | '\u09f3' | '\u0e3f' | '\u17db' | '\u20a0'..'\u20b1'
    |   '\ufdfc' | '\ufe69' | '\uff04' | '\uffe0' | '\uffe1' | '\uffe5' | '\uffe6'
    ;

fragment Exponent
    :   'e' ('+' | '-')? Digit+
    ;

Number
    :   Digit+ { $type = Integer; }
        ('.' Digit* { $type = Decimal; })?
        (Exponent { $type = Real; })?
    |   '.' { $type = DOT; }
        (
                Digit+ { $type = Decimal; }
                Exponent? { $type = Real; }
        )?
    |   Currency Digit+ '.' Digit* { $type = Money; }
    |   Currency '.' Digit+ { $type = Money; }
    |   Currency Digit+ { $type = Money; }
    |   '0x' ('a'..'f' | Digit)* { $type = HexLiteral; } // "0x" is valid hex literal
    ;

DateTime
    :   '{' (NewLine | Space)? ('ts' | 't' | 'd') (NewLine | Space)?
        'n'? '\'' (~'\'')* '\'' ('\'' (~'\'')* '\'')* (NewLine | Space)? '}'
    ;

PlainIdentifier
    :   ('a'..'z' | '_' | '#' | '\u0080'..'\ufffe') (Letter | Digit)* // first char other than '@'
    ;

BracketedIdentifier
    :   ('[' (~']')* ']')
        {
            // Remove leading [ and trailing ].
            $text = $text.Substring(1, $text.Length - 2);
        }
    ;

QuotedIdentifier
    :   ('"' (~'"')* '"')+
        {
            // Remove leading and trailing ", replace internal "" by ".
            $text = $text.Substring(1, $text.Length - 2).Replace("\"\"", "\"");
        }
    ;

Variable
    :   '@' (Letter | Digit)+
        {
            // Remove leading @.
            $text = $text.Substring(1);
        }
    ;

ASCIIStringLiteral
    :   ('\'' (~'\'')* '\'')+
        {
            // Remove leading and trailing ', replace internal '' by '.
            $text = $text.Substring(1, $text.Length - 2).Replace("''", "'");
        }
    ;

UnicodeStringLiteral
    :   'n' ('\'' (~'\'')* '\'')+
        {
            // Remove the "n", leading and trailing ', replace internal '' by '.
            $text = $text.Substring(2, $text.Length - 3).Replace("''", "'");
        }
    ;
