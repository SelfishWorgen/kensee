<?php

	include ('constants.php');
	require_once('include/EXCEL/PHPExcel.php');

	$sql = $_POST['sql'];
	$res = mysql_query($sql);
	$arr = array();
	while ($arr1 = mysql_fetch_array($res))
	{
		$arr1['site'] = $arr1['company_name'];
		if ($arr1['news_date'] != 'N/A')
			$arr1['dtt'] = date('M d, Y',strtotime($arr1['news_date']));
		else
			$arr1['dtt'] = 'N/A';
		array_push($arr, $arr1);
	}

    $userColumns = [];
    $u_clmn_sql = 'select * from tb_columns where user_id = '.$_SESSION['USER_ID'];
    $res1 = mysql_query($u_clmn_sql);
    $uClmnData = array();
 	while ($arr2 = mysql_fetch_array($res1))
    {
		array_push($uClmnData, $arr2);
    }
                 
	$objPHPExcel = new PHPExcel();

    if (count($uClmnData) == '0')
    {
	   $objPHPExcel->setActiveSheetIndex(0)
						->setCellValue('A1', 'Source')
						->setCellValue('B1', 'Country')
						->setCellValue('C1', 'Event')
						->setCellValue('D1', 'HeadLine')
                       	->setCellValue('E1', 'Sentiment')
						->setCellValue('F1', 'URL');
    }
    else
    {
        $objPHPExcel->setActiveSheetIndex(0)
						->setCellValue('A1', 'Source')
						->setCellValue('B1', 'Country')
						->setCellValue('C1', 'Event')
						->setCellValue('D1', 'HeadLine')
						->setCellValue('E1', 'Sentiment')
                        ->setCellValue('F1', $uClmnData[0]['column_name'])
						->setCellValue('G1', 'Url');
    }
    

	for ($j = 0, $k = 2; $j < count($arr); $j++, $k++)
	{
		$item = $arr[$j];

		$news_title = iconv('windows-1251', 'utf-8', $item['news_title']);
		$news_date = $item['dtt'];
		$news_url = $item['news_url'];
		$company_name = $item['company_name'];
		$event = $item['event'];
		$country = $item['country'];
		$sector = $item['sector'];
        $sentiment = $item['sentiment'];

        if (count($uClmnData) == '0')
        {
		  $objPHPExcel->setActiveSheetIndex(0)
						->setCellValue('A'.$k, $company_name)
						->setCellValue('B'.$k, $country)
						->setCellValue('C'.$k, $event)
						->setCellValue('D'.$k, $news_title)
						->setCellValue('E'.$k, $sentiment)
                        ->setCellValue('F'.$k, $news_url);
        }
        else
        {
            $u_clmn_data_sql = 'select * from tb_user_data where news_id = '.$item['id'] .' and column_id = '.$uClmnData[0]['id'];
            $res3 = mysql_query($u_clmn_data_sql);
 			$arr3 = mysql_fetch_array($res3);
            $txt = '';
            if ($arr3 != null)
                $txt = $arr3['data_text'];
            $objPHPExcel->setActiveSheetIndex(0)
						->setCellValue('A'.$k, $company_name)
						->setCellValue('B'.$k, $country)
						->setCellValue('C'.$k, $event)
						->setCellValue('D'.$k, $news_title)
                        ->setCellValue('E'.$k, $sentiment)
                        ->setCellValue('F'.$k, $txt)
                        ->setCellValue('G'.$k, $news_url);
        }
	}

	foreach(range('A','F') as $columnID) {
		$objPHPExcel->getActiveSheet()->getColumnDimension($columnID)
			->setAutoSize(true);
	}

	$objPHPExcel->getActiveSheet()->setTitle('News_'.date('Ymd'));


	// Set active sheet index to the first sheet, so Excel opens this as the first sheet
	$objPHPExcel->setActiveSheetIndex(0);

	$fileName = 'News_'.date('Ymd').'.xlsx';
	// Redirect output to a client¡¯s web browser (Excel5)
	header('Content-Type: application/vnd.ms-excel');
	header('Content-Disposition: attachment;filename="'.$fileName.'"');
	header('Cache-Control: max-age=0');
	// If you're serving to IE 9, then the following may be needed
	header('Cache-Control: max-age=1');

	// If you're serving to IE over SSL, then the following may be needed
	header ('Expires: Mon, 26 Jul 1997 05:00:00 GMT'); // Date in the past
	header ('Last-Modified: '.gmdate('D, d M Y H:i:s').' GMT'); // always modified
	header ('Cache-Control: cache, must-revalidate'); // HTTP/1.1
	header ('Pragma: public'); // HTTP/1.0

	$objWriter = PHPExcel_IOFactory::createWriter($objPHPExcel, 'Excel2007');
	$objWriter->save('php://output');

	//header('Location: index.php');
?>