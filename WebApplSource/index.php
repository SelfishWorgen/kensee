<?php
	include ('constants.php');
	include ('include/encrypt.php');
	security_check();

	function dt_adjust($dt)
	{
		//$dt = 'Jan, 2014';
		$dt1 = $dt;
		$dt1 = str_replace('Jan', 'Jan 01', $dt1);
		$dt1 = str_replace('Feb', 'Feb 01', $dt1);
		$dt1 = str_replace('Mar', 'Mar 01', $dt1);
		$dt1 = str_replace('Apr', 'Apr 01', $dt1);
		$dt1 = str_replace('May', 'May 01', $dt1);
		$dt1 = str_replace('Jun', 'Jun 01', $dt1);
		$dt1 = str_replace('Jul', 'Jul 01', $dt1);
		$dt1 = str_replace('Aug', 'Aug 01', $dt1);
		$dt1 = str_replace('Sep', 'Sep 01', $dt1);
		$dt1 = str_replace('Oct', 'Oct 01', $dt1);
		$dt1 = str_replace('Nov', 'Nov 01', $dt1);
		$dt1 = str_replace('Dec', 'Dec 01', $dt1);

		return $dt1;
	}

	function getTypeFromSitename($site)
	{
		$type = '';
		if ($site == 'REIT news')
			$type = '1';
		else if ($site == 'Yahoo')
			$type = '2';
		else if ($site == 'Prnewswire')
			$type = '3';
		else if ($site == 'Plaza')
			$type = '4';
		else if ($site == 'First Capital')
			$type = '5';
		else if ($site == 'CT REIT')
			$type = '6';
		else if ($site == 'Riocan')
			$type = '7';
		else if ($site == 'SNL')
			$type = '8';
		else if ($site == 'Choice RE')
			$type = '9';
		else if ($site == 'Calloway REIT')
			$type = '10';

		return $type;
	}

	function getSitenameFromType($type)
	{
		$site = '';
		if ($type == '1')
			$site = 'REIT news';
		else if ($type == '2')
			$site = 'Yahoo';
		else if ($type == '3')
			$site = 'Prnewswire';
		else if ($type == '4')
			$site = 'Plaza';
		else if ($type == '5')
			$site = 'First Capital';
		else if ($type == '6')
			$site = 'CT REIT';
		else if ($type == '7')
			$site = 'Riocan';
		else if ($type == '8')
			$site = 'SNL';
		else if ($type == '9')
			$site = 'Choice RE';
		else if ($type == '10')
			$site = 'Calloway REIT';

		return $site;
	}

	function array2json($arr)
	{ 
		if(function_exists('json_encode')) 
			return json_encode($arr); //Lastest versions of PHP already has this functionality.
		$parts = array(); 
		$is_list = false; 

		//Find out if the given array is a numerical array 
		$keys = array_keys($arr); 
		$max_length = count($arr)-1; 
		if(($keys[0] == 0) and ($keys[$max_length] == $max_length))
		{//See if the first key is 0 and last key is length - 1
			$is_list = true; 
			for($i=0; $i<count($keys); $i++)
			{ //See if each key correspondes to its position 
				if($i != $keys[$i])
				{ //A key fails at position check. 
					$is_list = false; //It is an associative array. 
					break; 
				} 
			} 
		} 

		foreach($arr as $key=>$value)
		{ 
			if(is_array($value))
			{ //Custom handling for arrays 
				if($is_list) 
					$parts[] = array2json($value); /* :RECURSION: */ 
				else 
					$parts[] = '"' . $key . '":' . array2json($value); /* :RECURSION: */ 
			}
			else
			{ 
				$str = ''; 
				if(!$is_list)
					$str = '"' . $key . '":'; 

				//Custom handling for multiple data types 
				if(is_numeric($value)) $str .= $value; //Numbers 
				elseif($value === false) $str .= 'false'; //The booleans 
				elseif($value === true) $str .= 'true'; 
				else $str .= '"' . addslashes($value) . '"'; //All other things 
				// :TODO: Is there any more datatype we should be in the lookout for? (Object?) 

				$parts[] = $str; 
			} 
		} 
		$json = implode(',',$parts); 

		if($is_list) 
			return '[' . $json . ']';//Return numerical JSON 
		return '{' . $json . '}';//Return associative JSON 
	}

	if (isset($_POST['postflag']))
	{
		$postflag = $_POST['postflag'];
		if ($postflag == '1')
		{
			//REMOVING DUPLICATES
//			$sql = 'DELETE tb_news FROM tb_news
//			LEFT OUTER JOIN (
//			   SELECT MIN(id) as RowId, news_url
//			   FROM tb_news
//			   GROUP BY news_url
//			) as KeepRows ON
//			   tb_news.id = KeepRows.RowId
//			WHERE
//			   KeepRows.RowId IS NULL';

//		   mysql_query($sql);

			$industry1 = $_POST['industry1'];
			$industry2 = $_POST['industry2'];

			$subwhere = '';
			if ($industry1 == '1' and $industry2 == '1')
				$subwhere = '(sector="Real Estate" or sector="Chemicals")';
			else if ($industry1 == '1' and $industry2 == '0')
				$subwhere = '(sector="Real Estate")';
			else if ($industry1 == '0' and $industry2 == '1')
				$subwhere = '(sector="Chemicals")';
			else if ($industry1 == '0' and $industry2 == '0')
				$subwhere = '(sector="Real Estate" and sector="Chemicals")';

			$sort = $_POST['sort'];
			$keyword = $_POST['keyword'];
			$pg = $_POST['page'];
			$up_down = $_POST['up_down'];

			$option1 = $_POST['option1'];
			$option2 = $_POST['option2'];
			$option3 = $_POST['option3'];
			$option4 = $_POST['option4'];
			$optionKind = $_POST['optionKind'];

			$optionArr1 = json_decode($option1, true);
			$optionArr2 = json_decode($option2, true);
			$optionArr3 = json_decode($option3, true);
			$optionArr4 = json_decode($option4, true);

            $checkedRows1 = $_POST['checkedRows'];
            $checkedRows = json_decode($checkedRows1, true);
            
			$itemNum = '';

			$sql = 'select count(*) as cnt from tb_news where '.$subwhere.' and news_title<>"" and news_title like "%'.$keyword.'%"';

			if ($optionKind == '4')
			{
				if (count($optionArr1) > 0 and count($optionArr2) > 0 and count($optionArr4) > 0)
				{
					$sql .= ' and (';

					foreach($optionArr1 as $key=>$val)
					{
						if ($val != 'N/A')
						{
							$param_dt = date('Y-m',strtotime(dt_adjust($val)));

							$min_dt = $param_dt.'-01';
							$max_dt = $param_dt.'-31';

							$sql .= '(news_date>="'.$min_dt.'" and news_date<="'.$max_dt.'") or ';
						}
						else
							$sql .= '(news_date>="N/A") or ';
					}
					
					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr2 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'company_name="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr3 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'event="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr4 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'country="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ')';
				}
			}

			$res = mysql_query($sql);
			$arr = mysql_fetch_array($res);

			$itemNum = intval($arr['cnt']);
			$pgNum = 0;
			if ($itemNum == 0)
				$pgNum = 0;
			else
			{
				$pgNum = intval($itemNum / $pgSize);
				if ($itemNum % $pgSize != 0)
					$pgNum++;
			}
			$navhtml = '';
			$navhtml1 = '';
            $userColumns = [];
            $u_clmn_sql = 'select * from tb_columns where user_id = '.$_SESSION['USER_ID'];
            $res = mysql_query($u_clmn_sql);
           	$uClmnData = array();
            while ($arr = mysql_fetch_array($res))
            {
		   	  array_push($uClmnData, $arr);
      		}
                
			if ($pgNum != 0)
			{           
                $navhtml = '<tbody><tr><td width="30%">Select records to export</td>';
                $navhtml .= '<td width="15%"><a href="javascript:void(0);" class="ph-button ph-btn-blue" onclick="export_excel();"><span style="color:#ffffff; font-size:14px;">Export</span></a>';
                if (count($uClmnData) < '1')
                    $navhtml .= '<td width="15%"><a href="javascript:void(0);" class="ph-button ph-btn-blue" onclick="add_column();"><span style="color:#ffffff; font-size:14px;">Add Column</span></a>';
				$navhtml .= '</td><td width="10%" vertical-align:"middle" align="right"><div class=" jcarousel-skin-tango"><div class="jcarousel-prev jcarousel-prev-horizontal" style="display: block;position:static;" onclick="prevPage();"></div></div></td><td id="pg_show" class="item_name" style="font-size:17px; color:rgb(63,133,169);" width="20%" align="center"><b>Page '.$pg.' of '.$pgNum.'</b></td><td width="10%" align="left"><div class=" jcarousel-skin-tango"><div class="jcarousel-next jcarousel-next-horizontal" style="display: block;position:static;" onclick="nextPage();"></div></div></td></tr></tbody>';
				$navhtml1 = '<tbody><tr><td width="60%">&nbsp;</td><td width="10%" align="right"><div class=" jcarousel-skin-tango"><div class="jcarousel-prev jcarousel-prev-horizontal" style="display: block;position:static;" onclick="prevPage();"></div></div></td><td id="pg_show" class="item_name" style="font-size:17px; color:rgb(63,133,169);" width="20%" align="center"><b>Page '.$pg.' of '.$pgNum.'</b></td><td width="10%" align="left"><div class=" jcarousel-skin-tango"><div class="jcarousel-next jcarousel-next-horizontal" style="display: block;position:static;" onclick="nextPage();"></div></div></td></tr></tbody>';
			}
			else
			{
				$navhtml = '<tr style="line-height:39px;"><td>&nbsp;</td></tr>';
				$navhtml1 = '<tr style="line-height:39px;"><td>&nbsp;</td></tr>';
			}
			
			$sortkey = '';
			if ($sort == '1')
				$sortkey = 'news_date';
			else if ($sort == '2')
				$sortkey = 'news_title';
			else if ($sort == '3')
				$sortkey = 'type';
			else if ($sort == '4')
				$sortkey = 'event';
			else if ($sort == '5')
				$sortkey = 'country';
			else if ($sort == '6')
				$sortkey = 'sentiment';
            else if ($sort == '7')
                return;
			//	$sortkey = 'sentiment';
			
			$up_down_key = '';
			if ($up_down == '2')
				$up_down_key = 'asc';
			else if ($up_down == '1')
				$up_down_key = 'desc';

			$start = (intval($pg) - 1) * $pgSize;
			$sql = 'select * from tb_news where '.$subwhere.' and news_title<>"" and news_title like "%'.$keyword.'%" ';

			if ($optionKind == '4')
			{
				if (count($optionArr1) > 0 and count($optionArr2) > 0 and count($optionArr4) > 0)
				{
					$sql .= ' and (';

					foreach($optionArr1 as $key=>$val)
					{
						if ($val != 'N/A')
						{
							$param_dt = date('Y-m',strtotime(dt_adjust($val)));

							$min_dt = $param_dt.'-01';
							$max_dt = $param_dt.'-31';

							$sql .= '(news_date>="'.$min_dt.'" and news_date<="'.$max_dt.'") or ';
						}
						else
							$sql .= '(news_date>="N/A") or ';
					}
					
					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr2 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'company_name="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr3 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'event="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr4 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'country="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ')';
				}
			}

			$sql .= ' order by '.$sortkey.' '.$up_down_key.' limit '.$start.','.$pgSize;
			$res = mysql_query($sql);
			$itemData = array();
            
 			while ($arr = mysql_fetch_array($res))
			{
				//$arr['site'] = getSitenameFromType($arr['type']);
				$arr['site'] = $arr['company_name'];
				if ($arr['news_date'] != 'N/A')
					$arr['dtt'] = date('M d, Y',strtotime($arr['news_date']));
				else
					$arr['dtt'] = 'N/A';
    			array_push($itemData, $arr);
      		}
            $col_sql = 'select * from tb_news where '.$subwhere.' and news_title<>"" and news_title like "%'.$keyword.'%" ';
			$html = '';
            $pageRowIds = [];
			if (count($itemData) == '0')
			{
				$html = '<tr class="" id="output4" rel="1455" style="opacity: 200; height:50px; cursor:pointer;" ><td colspan="6" align="center">No News</td></tr>';
			}
			else
			{
                $i = 0;
                foreach($uClmnData as $item)
                {
                    $userColumns[$i] = $item['column_name'];
                    $i = $i + 1;
                }
			    $i = 0;
				foreach($itemData as $item)
				{
				    $pageRowIds[$i] = $item['id'];
					$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer;" >';
                    $html .= '<td valign="top" style="weight:10;"><input type="checkbox"';
                    if (count($checkedRows) != '0' && in_array($item['id'], $checkedRows))
                        $html .= ' checked';
                    $html .= ' id="chk_5_'.$item['id'].'"/></td>';
                    $html .= '<td valign="top">'.$item['dtt'].'</td><td valign="top"><a href="javascript:void(0);" onclick="show_content('.$item['id'].');">'.$item['news_title'].'</a></td>';
					$html .= '<td valign="top">'.$item['site'].'</td><td valign="top">'.$item['event'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['sentiment'].'</td>';
                    foreach($uClmnData as $item1)
                    {
                        $u_clmn_data_sql = 'select * from tb_user_data where news_id = '.$item['id'] .' and column_id = '.$item1['id'];
                        $res = mysql_query($u_clmn_data_sql);
 			            $arr = mysql_fetch_array($res);
                        if ($arr != null)
                            $html .= '<td valign="top"><a href="javascript:void(0);" onclick="change_column_data('.$arr['id'].');" style="text-align:right;">'.$arr['data_text'].'</a></td>';
                        else
                            $html .= '<td valign="top"><a href="javascript:void(0);" onclick="add_column_data('.$item['id'].','.$item1['id'].');" style="text-align:right;color:#00A2E8">add...</a></td>';
                    }
                    $html .= '</tr>';
				    $i = $i + 1;
                }
			}

			//FILTER SEARCH
			$news_date_filter1 = array();
			$news_date_filter = array();
			if ($option1 == '')
			{
				$sql = 'select news_date from tb_news where '.$subwhere.' and  news_title<>"" and news_title like "%'.$keyword.'%" ';
				if ($optionKind == '4')
				{
					if (count($optionArr1) > 0 and count($optionArr2) > 0 and count($optionArr4) > 0)
					{
						$sql .= ' and (';

						foreach($optionArr1 as $key=>$val)
						{
							if ($val != 'N/A')
							{
								$param_dt = date('Y-m',strtotime(dt_adjust($val)));

								$min_dt = $param_dt.'-01';
								$max_dt = $param_dt.'-31';

								$sql .= '(news_date>="'.$min_dt.'" and news_date<="'.$max_dt.'") or ';
							}
							else
								$sql .= '(news_date>="N/A") or ';
						}
						
						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr2 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'company_name="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr3 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'event="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr4 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'country="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ')';
					}
				}

				$sql .= 'group by news_date order by news_date desc';
				$res = mysql_query($sql);
				
				while ($arr = mysql_fetch_array($res))
				{
					if ($arr['news_date'] != 'N/A')
						$arr['dtt'] = date('M, Y',strtotime($arr['news_date']));
					else
						$arr['dtt'] = 'N/A';
					array_push($news_date_filter1, $arr['dtt']);
				}
				$news_date_filter1 = array_unique($news_date_filter1);

				foreach($news_date_filter1 as $key=>$val)
					array_push($news_date_filter, $val);
			}
			else
			{
				foreach($optionArr1 as $key=>$val)
					array_push($news_date_filter, $val);
			}

			//FILTER SEARCH
			$news_source_filter = array();
			if ($option2 == '')
			{
				$sql = 'select company_name from tb_news where '.$subwhere.' and news_title<>"" and news_title like "%'.$keyword.'%" ';
				if ($optionKind == '4')
				{
					if (count($optionArr1) > 0 and count($optionArr2) > 0 and count($optionArr4) > 0)
					{
						$sql .= ' and (';

						foreach($optionArr1 as $key=>$val)
						{
							if ($val != 'N/A')
							{
								$param_dt = date('Y-m',strtotime(dt_adjust($val)));

								$min_dt = $param_dt.'-01';
								$max_dt = $param_dt.'-31';

								$sql .= '(news_date>="'.$min_dt.'" and news_date<="'.$max_dt.'") or ';
							}
							else
								$sql .= '(news_date>="N/A") or ';
						}
						
						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr2 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'company_name="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr3 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'event="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr4 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'country="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ')';
					}
				}
				$sql .= 'group by company_name order by company_name asc';
				$res = mysql_query($sql);
				
				while ($arr = mysql_fetch_array($res))
					array_push($news_source_filter, $arr['company_name']);
					//array_push($news_source_filter, getSitenameFromType($arr['type']));
			}
			else
			{
				foreach($optionArr2 as $key=>$val)
					array_push($news_source_filter, $val);
			}

			//FILTER SEARCH
			$news_country_filter = array();
			if ($option4 == '')
			{
				$sql = 'select country from tb_news where '.$subwhere.' and news_title<>"" and news_title like "%'.$keyword.'%" ';
				if ($optionKind == '4')
				{
					if (count($optionArr1) > 0 and count($optionArr2) > 0 and count($optionArr4) > 0)
					{
						$sql .= ' and (';

						foreach($optionArr1 as $key=>$val)
						{
							if ($val != 'N/A')
							{
								$param_dt = date('Y-m',strtotime(dt_adjust($val)));

								$min_dt = $param_dt.'-01';
								$max_dt = $param_dt.'-31';

								$sql .= '(news_date>="'.$min_dt.'" and news_date<="'.$max_dt.'") or ';
							}
							else
								$sql .= '(news_date>="N/A") or ';
						}
						
						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr2 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'company_name="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr3 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'event="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr4 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'country="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ')';
					}
				}
				$sql .= 'group by country order by country asc';
				$res = mysql_query($sql);
				
				$news_country_filter1 = array();
				while ($arr = mysql_fetch_array($res))
					array_push($news_country_filter1, str_replace(' ', '', utf8_encode($arr['country'])));
				$news_country_filter1 = array_unique($news_country_filter1);
				foreach($news_country_filter1 as $key=>$val)
					array_push($news_country_filter, $val);
					//array_push($news_source_filter, getSitenameFromType($arr['type']));
			}
			else
			{
				foreach($optionArr4 as $key=>$val)
					array_push($news_country_filter, $val);
			}

			//EVENT FILTER SEARCH
			$news_event_filter = array();
			if ($option3 == '')
			{
				$sql = 'select event from tb_news where '.$subwhere.' and news_title<>"" and news_title like "%'.$keyword.'%" ';
				if ($optionKind == '4')
				{
					if (count($optionArr1) > 0 and count($optionArr2) > 0 and count($optionArr4) > 0)
					{
						$sql .= ' and (';

						foreach($optionArr1 as $key=>$val)
						{
							if ($val != 'N/A')
							{
								$param_dt = date('Y-m',strtotime(dt_adjust($val)));

								$min_dt = $param_dt.'-01';
								$max_dt = $param_dt.'-31';

								$sql .= '(news_date>="'.$min_dt.'" and news_date<="'.$max_dt.'") or ';
							}
							else
								$sql .= '(news_date>="N/A") or ';
						}
						
						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr2 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'company_name="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr3 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'event="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ') and (';

						foreach($optionArr4 as $key=>$val)
						{
							//$param_source = getTypeFromSitename($val);
							$sql .= 'country="'.$val.'" or ';
						}

						$sql = substr($sql, 0, strlen($sql) - 3);
						$sql .= ')';
					}
				}
				$sql .= 'group by event order by event asc';
				$res = mysql_query($sql);
				
				while ($arr = mysql_fetch_array($res))
					array_push($news_event_filter, $arr['event']);
			}
			else
			{
				foreach($optionArr3 as $key=>$val)
					array_push($news_event_filter, $val);
			}

			$ret = array();
			$ret['itemNum'] = $itemNum;
			$ret['pgNum'] = $pgNum;
			$ret['html'] = $html;
			$ret['navhtml'] = $navhtml;
			$ret['navhtml1'] = $navhtml1;
			$ret['optionKind'] = $optionKind;
			$ret['news_date_filter'] = array2json($news_date_filter);
			$ret['news_source_filter'] = array2json($news_source_filter);
			$ret['news_event_filter'] = array2json($news_event_filter);
			$ret['news_country_filter'] = array2json($news_country_filter);
            $ret['pageRowIds'] = array2json($pageRowIds);
            $ret['checkedRows'] = array2json($checkedRows);
            $ret['userColumns'] = array2json($userColumns);
			$ret = array_map('utf8_encode', $ret);
            
			$json = array2json($ret);
			print_r($json);exit;
		}
		else if ($postflag == '2')
		{
			$id = $_POST['id'];
			$sql = 'select * from tb_news where id="'.$id.'"';
			$res = mysql_query($sql);
			$arr = mysql_fetch_array($res);
			$arr['dtt'] = date('F d, Y',strtotime($arr['news_date']));

			$html = '<div id="ccc">';
			$html .= '<h4 style="color:green"><a href="'.$arr['news_url'].'" target="_blank">'.utf8_encode($arr['news_title']).'</a></h4>';
			$html .= '<h5 style="Color:#00A2E8">Published Date : '.$arr['dtt'].'</h5>';
			$html .= '<div style="font-size:13px;">'.utf8_encode(substr($arr['news_content'],0,3500).'<BR>...').'</div></div>';

			echo $html; exit;
		}
		else if ($postflag == '4') // before add column
		{
            $sql = 'select * from tb_columns where user_id = '.$_SESSION['USER_ID'];
            $res = mysql_query($sql);
 			$arr = mysql_fetch_array($res);
            if ($arr != null)
            {
                $json = array2json($arr);
                print_r($json);
            }
            else 
                echo ''; 
            exit;
		}
		else if ($postflag == '5') // after add column
		{
			$id = $_POST['id'];
			$column_name = $_POST['column_name'];
            if ($id == 0)
            {
                $sql = 'insert into tb_columns set ';
                $sql .= 'user_id='.$_SESSION['USER_ID'].',';
                $sql .= 'column_name="'.$column_name.'"';
            }
            else
            {
			   $sql = 'update tb_columns set column_name="'.$column_name.'" where id="'.$id.'"';
			}
            $res = mysql_query($sql);
			exit;
		}
        else if ($postflag == '6') // before add column data
		{
			$id = $_POST['id'];
			$sql = 'select * from tb_user_data where id="'.$id.'"';
			$res = mysql_query($sql);
			$arr = mysql_fetch_array($res);
			$cont = $arr['data_text'];
			echo $cont; 
            exit;
		}
		else if ($postflag == '7') // after add column data
		{
			$column_id = $_POST['column_id']; 
            $news_id = $_POST['news_id'];
            $id = $_POST['id'];
			$data_text = $_POST['data_text'];
            if ($id == 0)
            {
                $sql = 'insert into tb_user_data set ';
                $sql .= 'column_id='.$column_id.',';
                $sql .= 'news_id='.$news_id.',';
                $sql .= 'data_text="'.$data_text.'"';
            }
            else
            {
			     $sql = 'update tb_user_data set data_text="'.$data_text.'" where id="'.$id.'"';
            }
			$res = mysql_query($sql);
			exit;
		}
        else if ($postflag == '8') // delete column
		{
            $id = $_POST['id'];
            $sql = 'delete from tb_user_data where column_id="'.$id.'"';
   			$res = mysql_query($sql);
            
            $sql = 'delete from tb_columns where id="'.$id.'"';
   			$res = mysql_query($sql);
            
			exit;
		}
        else if ($postflag == '9') // delete column data
		{
            $id = $_POST['id'];    
            $sql = 'delete from tb_user_data where id="'.$id.'"';
   			$res = mysql_query($sql);
			exit;
		}
        else if ($postflag == '3') // export to Excel
		{
			//REMOVING DUPLICATES
			$sql = 'DELETE tb_news FROM tb_news
			LEFT OUTER JOIN (
			   SELECT MIN(id) as RowId, news_url
			   FROM tb_news
			   GROUP BY news_url
			) as KeepRows ON
			   tb_news.id = KeepRows.RowId
			WHERE
			   KeepRows.RowId IS NULL';

		   mysql_query($sql);

			$industry1 = $_POST['industry1'];
			$industry2 = $_POST['industry2'];
            $checkedRows = $_POST['checkedRows'];
            $checkedRows = json_decode($checkedRows, true);

			$subwhere = '';
			if ($industry1 == '1' and $industry2 == '1')
				$subwhere = '(sector="Real Estate" or sector="Chemicals")';
			else if ($industry1 == '1' and $industry2 == '0')
				$subwhere = '(sector="Real Estate")';
			else if ($industry1 == '0' and $industry2 == '1')
				$subwhere = '(sector="Chemicals")';
			else if ($industry1 == '0' and $industry2 == '0')
				$subwhere = '(sector="Real Estate" and sector="Chemicals")';

			$sort = $_POST['sort'];
			$keyword = $_POST['keyword'];
			$up_down = $_POST['up_down'];

			$option1 = $_POST['option1'];
			$option2 = $_POST['option2'];
			$option3 = $_POST['option3'];
			$option4 = $_POST['option4'];
			$optionKind = $_POST['optionKind'];

			$optionArr1 = json_decode($option1, true);
			$optionArr2 = json_decode($option2, true);
			$optionArr3 = json_decode($option3, true);
			$optionArr4 = json_decode($option4, true);
			
			$sortkey = '';
			if ($sort == '1')
				$sortkey = 'news_date';
			else if ($sort == '2')
				$sortkey = 'news_title';
			else if ($sort == '3')
				$sortkey = 'type';
			else if ($sort == '4')
				$sortkey = 'event';
			else if ($sort == '5')
				$sortkey = 'country';
            else if ($sort == '6')
				$sortkey = 'sentiment';
            else if ($sort == '7')
				return;
			$up_down_key = '';
			if ($up_down == '2')
				$up_down_key = 'asc';
			else if ($up_down == '1')
				$up_down_key = 'desc';

			$sql = 'select id, news_title, news_date, news_url, company_name, event, country, sector, sentiment from tb_news where '.$subwhere.' and news_title<>"" and news_title like "%'.$keyword.'%" ';
//           	$sql = 'select news_title, news_date, company_name, event, country  from tb_news where news_title<>"" and news_title like "%'.$keyword.'%" ';
            
            if (count($checkedRows) > 0)
            {
                $sql .= ' and (';
                foreach($checkedRows as $val)
                {
                    $sql .= 'id="'.$val.'" or ';
                }
                $sql = substr($sql, 0, strlen($sql) - 3);
                $sql .= ')';
            }
			if ($optionKind == '4')
			{
				if (count($optionArr1) > 0 and count($optionArr2) > 0 and count($optionArr4) > 0)
				{
					$sql .= ' and (';

					foreach($optionArr1 as $key=>$val)
					{
						if ($val != 'N/A')
						{
							$param_dt = date('Y-m',strtotime(dt_adjust($val)));

							$min_dt = $param_dt.'-01';
							$max_dt = $param_dt.'-31';

							$sql .= '(news_date>="'.$min_dt.'" and news_date<="'.$max_dt.'") or ';
						}
						else
							$sql .= '(news_date>="N/A") or ';
					}
					
					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr2 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'company_name="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr3 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'event="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ') and (';

					foreach($optionArr4 as $key=>$val)
					{
						//$param_source = getTypeFromSitename($val);
						$sql .= 'country="'.$val.'" or ';
					}

					$sql = substr($sql, 0, strlen($sql) - 3);
					$sql .= ')';
				}
			}

			$sql .= ' order by '.$sortkey.' '.$up_down_key;

			echo $sql; exit;
		}
	}
?>

<html xmlns="http://www.w3.org/1999/xhtml"><head>

<meta http-equiv="Content-Type" content="text/html; charset=utf-8">

<title>Kensee</title>

<meta name="description" content="Compare book prices and promotions from major Singapore and international online bookstores including Amazon, Kinokuniya and The Book Depository. Buy books for the best prices all from one page.">

<meta name="keywords" content="">

<link rel="canonical" href="">

<link rel="author" href="https://plus.google.com/117465813919040222750/about">

<link href="<?php echo $SITEURL; ?>/css/bootstrap.css" type="text/css" rel="stylesheet">

<link rel="stylesheet" type="text/css" href="<?php echo $SITEURL; ?>/css/skin.css">

<link rel="stylesheet" type="text/css" href="<?php echo $SITEURL; ?>/css/dropdown.css">

<link href="<?php echo $SITEURL; ?>/css/style.css" type="text/css" rel="stylesheet">

<link href="<?php echo $SITEURL; ?>/css/tgn0.0.1.css" type="text/css" rel="stylesheet">

<link href="<?php echo $SITEURL; ?>/css/BackToTop.jquery.css" type="text/css" rel="stylesheet">

<link rel="shortcut icon" type="image/jpg" href="<?php echo $SITEURL; ?>/images/Kensee_Favicon.png">

<link rel="icon" type="image/jpg" href="<?php echo $SITEURL; ?>/images/Kensee_Favicon.png">

<link href="<?php echo $SITEURL; ?>/css/c3.css" rel="stylesheet" type="text/css">

<script type="text/javascript" async="" src="http://www.google-analytics.com/plugins/ga/inpage_linkid.js" id="undefined"></script><script type="text/javascript" async="" src="http://stats.g.doubleclick.net/dc.js"></script><script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery-ui.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery.validate.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/bootstrap.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery.jcarousel.min.js"></script>

<script type="text/javascript" src="http://d3js.org/d3.v3.min.js" charset="utf-8"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/c3.js"></script>

<script type="text/javascript">

<!--


  var _gaq = _gaq || [];

  var pluginUrl = '//www.google-analytics.com/plugins/ga/inpage_linkid.js';

  _gaq.push(['_require', 'inpage_linkid', pluginUrl]);

  _gaq.push(['_setAccount', 'UA-33516758-1']);

  _gaq.push(['_setDomainName', 'oo.sg']);

  _gaq.push(['_trackPageview']);

  (function() {

    var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;

    ga.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'stats.g.doubleclick.net/dc.js';

    var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);

  })(); 


  var modal_visible = false;

  var site_url = '';

//-->

</script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/script.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/common.js"></script>


	<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery.nivo.slider.js"></script>
	<link href="css/nivo/nivo-slider.css" type="text/css" rel="stylesheet">
	<link rel="stylesheet" href="css/nivo/default.css" type="text/css" media="screen">
	<script type="text/javascript">
	<!--
    $(document).ready(function() {
		$(window).load(function() {
			/*var total = $('#slider img').length;
			var rand = Math.floor(Math.random()*total);
		    $('#slider').nivoSlider({
				startSlide:rand
		    });		*/	
			$('#slider-preload').hide();
			$('#slider').nivoSlider({
				effect : 'fade',
				controlNav : true,
				directionNav : true,
				pauseOnHover: false,
				pauseTime: 5000
			});
		});
		
		$('#top10_searches .tab_links > a').click(function() {
			$('#top10_searches a').removeClass('selected');
			$('#top10_searches > div').hide();
			$('.tab_links span').hide();
			
			$($(this).attr('href')).show();
			$($(this).attr('href')+'_date').show();
			$(this).addClass('selected');
			
			return false;
		});
				$('#column_right .content').css('cursor', 'pointer');
		$('#column_right .content').click(function() {		
			document.location.href = '';
		});
			});
	//-->
    </script>
    <style type="text/css">
	#display{
		border:thin solid #f1f1f1;
		border-top:none;
	}
	.ph-float {
	  padding: 10px;  
	  padding-top: 50px;  
	}
	.ph-btn-green {
		border-color: #3AC162;
		background-color: #5FCF80;

	}
	.ph-btn-green:hover, .ph-btn-green:focus, .ph-btn-green:active {
		background-color: #3AC162;
		border-color: #3AC162;
	}

	.ph-btn-blue {
		border-color: #00A2E8;
		background-color: #00A2E8;

	}
	.ph-btn-blue:hover, .ph-btn-blue:focus, .ph-btn-blue:active {
		background-color: #20BCFF;
		border-color: #20BCFF;
	}

	.ph-button {
		
		border-style: solid;
		border-width: 0px 0px 3px;
		box-shadow: 0 -1px 0 rgba(255, 255, 255, 0.1) inset;
		color: #FFFFFF;	   
		border-radius: 12px;
		cursor: pointer;
		display: inline-block;
		font-style: normal;
		overflow: hidden;
		text-align: center;
		text-decoration: none;
		text-overflow: ellipsis;
		transition: all 200ms ease-in-out 0s;
		white-space: nowrap;	
		font-family: "Gotham Rounded A","Gotham Rounded B",Helvetica,Arial,sans-serif;
		font-weight: 700;	
		padding: 8px 10px 8px;
		font-size: 18px;
	}

	.arrow-up {
 	top:168px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-bottom: 10px solid #fff;
		display:inline;
		position:absolute;
	}

	.arrow-up-color {
		top:168px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-bottom: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}

	.arrow-up:hover {
		top:168px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-bottom: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}

	.arrow-down {
		top:168px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-top: 10px solid #fff;
		display:inline;
		position:absolute;
	}
	.arrow-down:hover {
		top:168px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-top: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}

	.arrow-down-color {
		top:168px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-top: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}

	#backgroundPopup {
		z-index:1;
		position: fixed;
		display:none;
		height:100%;
		width:100%;
		background:#000000;
		top:0px;
		left:0px;
	}
	#toPopup {
		font-family: "lucida grande",tahoma,verdana,arial,sans-serif;
		background: none repeat scroll 0 0 #FFFFFF;
		border: 10px solid #ccc;
		border-radius: 3px 3px 3px 3px;
		color: #333333;
		display: none;
		font-size: 12px;
		left: 50%;
		margin-left: -270px;
		position: fixed;
		top: 34%;
		width: 520px;
		z-index: 2;
	}
	div.loader {
		background: url("img/loading3.gif") no-repeat scroll 0 0 transparent;
		height: 32px;
		width: 32px;
		display: none;
		z-index: 9999;
		top: 40%;
		left: 50%;
		position: absolute;
		margin-left: -10px;
	}
	div.close {
		background: url("img/closebox.png") no-repeat scroll 0 0 transparent;
		cursor: pointer;
		height: 30px;
		position: absolute;
		right: -27px;
		top: -24px;
		width: 30px;
	}
	span.ecs_tooltip {
		background: none repeat scroll 0 0 #000000;
		border-radius: 2px 2px 2px 2px;
		color: #FFFFFF;
		display: none;
		font-size: 11px;
		height: 16px;
		opacity: 0.7;
		padding: 4px 3px 2px 5px;
		position: absolute;
		right: -62px;
		text-align: center;
		top: -51px;
		width: 93px;
	}
	span.arrow {
		border-left: 5px solid transparent;
		border-right: 5px solid transparent;
		border-top: 7px solid #000000;
		display: block;
		height: 1px;
		left: 40px;
		position: relative;
		top: 3px;
		width: 1px;
	}
	div#popup_content {
		width:500px;
		margin: 4px 7px;
		/* remove this comment if you want scroll bar
		overflow-y:scroll;
		height:200px
		*/
	}
	</style>

	<link rel="stylesheet" href="<?php echo $SITEURL; ?>/css/search_form.css" type="text/css">

<script>

$(function(){
	/*console.log($(window).height());
	console.log($(document).height());
	$('<span class="tooltip"></span>')
        .html('AAAA')
        .appendTo('body');
	$('.tooltip').css({ top: $(document).height(), left: 100 })*/

	$("div.close").hover(
                    function() {
                        $('span.ecs_tooltip').show();
                    },
                    function () {
                        $('span.ecs_tooltip').hide();
                      }
                );
    
    $("div.close").click(function() {
        disablePopup();  // function close pop up
    });
    
    $(this).keyup(function(event) {
        if (event.which == 27) { // 27 is 'Ecs' in the keyboard
            disablePopup();  // function close pop up
        }      
    });
    
    $("div#backgroundPopup").click(function() {
        disablePopup();  // function close pop up
    });
});

function loading() {
	$("div.loader").show();  
}

function closeloading() {
	$("div.loader").fadeOut('normal');  
}

var popupStatus = 0; // set value

function loadPopup() {
	if(popupStatus == 0) { // if value is 0, show popup
		closeloading(); // fadeout loading
		$("#toPopup").fadeIn(0500); // fadein popup div
		$("#backgroundPopup").css("opacity", "0.7"); // css opacity, supports IE7, IE8
		$("#backgroundPopup").fadeIn(0001);
		popupStatus = 1; // and set value to 1
	}    
}

function disablePopup() {
	if(popupStatus == 1) { // if value is 1, close popup
		$("#toPopup").fadeOut("normal");  
		$("#backgroundPopup").fadeOut("normal");  
		popupStatus = 0;  // and set value to 0
	}
}

function check()
{
	var industry1 = 1, industry2 = 1;
//	if ($('#chk_industry1').is(':checked') == false)
//		industry1 = 0;
//	if ($('#chk_industry2').is(':checked') == false)
//		industry2 = 0;
	
	var sort = $('#sort').val();
	$('#cur_pg').val('1');
	var keyword = $('#keyword').val();
	$('#pg_Nav').html('<tr style="line-height:39px;"><td>&nbsp;</td></tr>');
	$('#pg_Nav1').html('<tr style="line-height:39px;"><td>&nbsp;</td></tr>');
	var up_down = $('#up_down').val();
	$('#sort').val('1');
	$('#col_ind').val('1');
	$('#up_down').val('2');
	$('#tbody_content').html('<tr class="output4 theStylingClass" style="cursor:pointer;"><td colspan="6" align="center"><img src="img/loading.gif"></td></tr>');
	$('#information').show();
    
    var userColumns = jQuery.parseJSON($('#userColumns').val());
    if (userColumns != null ) //&& userColumns.length != 0)
        document.getElementById('head7').style.display = 'inline';
    else
        document.getElementById('head7').style.display = 'none';
                                                
	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'1',keyword:keyword, page:'1', sort:'1', up_down:'1', checkedRows:'', option1:'', option2:'', option3:'', option4:'', optionKind:'', industry1:industry1, industry2:industry2},
		success : function(data){
			console.log(data);
			processJson(data);
			$('#head1').html('<span style="font-weight:bold; color:#00A2E8"><a href="javascript:void(0);" onclick="openFilter(\'1\');">Date</a></span>&nbsp;&nbsp;<div class="arrow-down-color" onclick="sort(\'1\');"></div>');
		}
	});
	return false;
}

function pg_Navigate()
{
	var industry1 = 1, industry2 = 1;
//	if ($('#chk_industry1').is(':checked') == false)
//		industry1 = 0;
//	if ($('#chk_industry2').is(':checked') == false)
//		industry2 = 0;

	//OPTION
	var opt_chk1 = [];
	var news_date_filter = jQuery.parseJSON($('#dt_filter_str1').val());
	for (i = 0; i < news_date_filter.length; i++)
	{
		opt_chk1[i] = news_date_filter[i];
	}
	var optStr1 = JSON.stringify(opt_chk1);
	
	var opt_chk2 = [];
	var ind = 0;
	var news_source_filter = jQuery.parseJSON($('#source_filter_str1').val());
	for (i = 0; i < news_source_filter.length; i++)
	{
		opt_chk2[i] = news_source_filter[i];
	}
	var optStr2 = JSON.stringify(opt_chk2);

	var opt_chk3 = [];
	var ind = 0;
	var news_event_filter = jQuery.parseJSON($('#event_filter_str1').val());
	for (i = 0; i < news_event_filter.length; i++)
	{
		opt_chk3[i] = news_event_filter[i];
	}
	var optStr3 = JSON.stringify(opt_chk3);

	var opt_chk4 = [];
	var ind = 0;
	var news_country_filter = jQuery.parseJSON($('#country_filter_str1').val());
	for (i = 0; i < news_country_filter.length; i++)
	{
		opt_chk4[i] = news_country_filter[i];
	}
	var optStr4 = JSON.stringify(opt_chk4);

	var optionKind = '4';

	var sort = $('#sort').val();
	var pg = $('#cur_pg').val();
	var keyword = $('#keyword').val();
	var up_down = 3 - parseInt($('#up_down').val());
    
    // checked row filter
    var checkedRows = [];
   	var checkedRows1 = jQuery.parseJSON($('#checkedRows').val());
    var n = 0;
    if (checkedRows1 != null)
    {
       n = checkedRows1.length;
	   for (i = 0; i < n; i++)
	     checkedRows[i] = checkedRows1[i];
    }
  	var pageRowIds = jQuery.parseJSON($('#pageRowIds').val());
 	for (i = 0; i < pageRowIds.length; i++)
	{
		var val = $('#chk_5_' + pageRowIds[i]).is(':checked');
        var x = checkedRows.indexOf(pageRowIds[i]);
		if (val == true)
		{
		  if (x == -1)
    	      checkedRows[n++] = pageRowIds[i];
		}
        else if (x != -1) 
            checkedRows.splice(x);
	}
 	var checkedRowsStr = JSON.stringify(checkedRows);
    
	$('#tbody_content').html('<tr class="output4 theStylingClass" style="cursor:pointer;"><td colspan="6" align="center"><img src="img/loading.gif"></td></tr>');
	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'1',keyword:keyword, page:pg, sort:sort, up_down:up_down, checkedRows:checkedRowsStr, option1:optStr1, option2:optStr2, option3:optStr3, option4:optStr4, optionKind:optionKind, industry1:industry1, industry2:industry2},
		success : function(data){
			console.log(data);
			processJson(data);
		}
	});
}

function processJson(data)
{
	var obj = jQuery.parseJSON(data);
	var html = obj.html;
	var itemNum = parseInt(obj.itemNum);
	var pgNum = parseInt(obj.pgNum);
	var navhtml = obj.navhtml;
	var navhtml1 = obj.navhtml1;

	if (obj.optionKind == '4')
	{
		$('#dt_filter_str1').val(obj.news_date_filter);
		$('#source_filter_str1').val(obj.news_source_filter);
		$('#event_filter_str1').val(obj.news_event_filter);
		$('#country_filter_str1').val(obj.news_country_filter);
	}
	else
	{
		$('#dt_filter_str').val(obj.news_date_filter);
		$('#source_filter_str').val(obj.news_source_filter);
		$('#dt_filter_str1').val(obj.news_date_filter);
		$('#source_filter_str1').val(obj.news_source_filter);
		$('#event_filter_str1').val(obj.news_event_filter);
		$('#event_filter_str').val(obj.news_event_filter);
		$('#country_filter_str').val(obj.news_country_filter);
		$('#country_filter_str1').val(obj.news_country_filter);
	}
    $('#pageRowIds').val(obj.pageRowIds);
    $('#checkedRows').val(obj.checkedRows);
    $('#userColumns').val(obj.userColumns);
    
    var userColumns = jQuery.parseJSON($('#userColumns').val());
    if (userColumns != null && userColumns.length != 0)
    {
        document.getElementById('head7').style.display = 'inline';
        $('#head7').html('<a href="javascript:void(0);" onclick="openUserColumn(1);">' + userColumns[0] + '</a>&nbsp;&nbsp');
    }
    else
    {
        document.getElementById('head7').style.display = 'none';
    }
    
    
    $('#itemNum').val(itemNum);
	$('#pgNum').val(pgNum);

	$('#tbody_content').html(html);
	$('#pg_Nav').html(navhtml);
	$('#pg_Nav1').html(navhtml1);
    
    var pageRowIds = jQuery.parseJSON($('#pageRowIds').val());
    var checkedRows = jQuery.parseJSON($('#checkedRows').val());
    var page_checked = false;
 	for (i = 0; i < pageRowIds.length; i++)
	{
	   if (checkedRows != null)
        for (i1 = 0; i1 < checkedRows.length; i1++)
        {
		  if (pageRowIds[i] == checkedRows[i1])
		  {
		    page_checked = true;
            break;
		  }
        }
	}
    if (page_checked)
    {
      $('#chk_page').attr('checked', 'checked');
    }
    else
    {
        $('#chk_page').removeAttr('checked');
    }
    
//    var chart = c3.generate({
//    bindto: '#chart',
//    data: {
//      columns: [
//        ['positive', 30, 200, 100, 400, 150, 250],
//        ['negative', 50, 20, 10, 40, 15, 25],
//        ['neutral', 5, 20, 10, 5, 15, 5]
//      ]
//    }
//  });
 }

function prevPage()
{
	var cur_pg = parseInt($('#cur_pg').val());
	var pgNum = parseInt($('#pgNum').val());
	if (cur_pg > 1)
	{
		cur_pg --; 
		$('#cur_pg').val(cur_pg);
		pg_Navigate();
	}
}

function nextPage()
{
	var cur_pg = parseInt($('#cur_pg').val());
	var pgNum = parseInt($('#pgNum').val());
	if (cur_pg < pgNum)
	{
		cur_pg ++; 
		$('#cur_pg').val(cur_pg);
		pg_Navigate();
	}
}

function show_content(id)
{
	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'2', id:id},
		success : function(data){
			detail_View(data);
		}
	});
}

function detail_View(data)
{
	$('.detailPanel').show();
	$('.detailPanel').html(data);
	$('.detailPanel').height($('#ccc').prop('scrollHeight') + 20);
	console.log($('#ccc').prop('scrollHeight') + 20);
	$("html, body").animate({ scrollTop: 0 }, "slow");
	return false;
}

function sort(val)
{
	var industry1 = 1, industry2 = 1;
//	if ($('#chk_industry1').is(':checked') == false)
//		industry1 = 0;
//	if ($('#chk_industry2').is(':checked') == false)
//		industry2 = 0;

	//OPTION
	var opt_chk1 = [];
	var news_date_filter = jQuery.parseJSON($('#dt_filter_str1').val());
	for (i = 0; i < news_date_filter.length; i++)
	{
		opt_chk1[i] = news_date_filter[i];
	}
	var optStr1 = JSON.stringify(opt_chk1);
	
	var opt_chk2 = [];
	var ind = 0;
	var news_source_filter = jQuery.parseJSON($('#source_filter_str1').val());
	for (i = 0; i < news_source_filter.length; i++)
	{
		opt_chk2[i] = news_source_filter[i];
	}
	var optStr2 = JSON.stringify(opt_chk2);

	var opt_chk3 = [];
	var ind = 0;
	var news_event_filter = jQuery.parseJSON($('#event_filter_str1').val());
	for (i = 0; i < news_event_filter.length; i++)
	{
		opt_chk3[i] = news_event_filter[i];
	}
	var optStr3 = JSON.stringify(opt_chk3);

	var opt_chk4 = [];
	var ind = 0;
	var news_country_filter = jQuery.parseJSON($('#country_filter_str1').val());
	for (i = 0; i < news_country_filter.length; i++)
	{
		opt_chk4[i] = news_country_filter[i];
	}
	var optStr4 = JSON.stringify(opt_chk4);

	var optionKind = '4';

	var cur_pg = parseInt($('#cur_pg').val());
	if (cur_pg == "-1")
	{
		return;
	}

    // checked row filter
    var checkedRows = [];
   	var checkedRows1 = jQuery.parseJSON($('#checkedRows').val());
    var n = 0;
    if (checkedRows1 != null)
    {
       n = checkedRows1.length;
	   for (i = 0; i < n; i++)
	     checkedRows[i] = checkedRows1[i];
    }
    var page_checked = false;
  	var pageRowIds = jQuery.parseJSON($('#pageRowIds').val());
 	for (i = 0; i < pageRowIds.length; i++)
	{
		var is_checked = $('#chk_5_' + pageRowIds[i]).is(':checked');
        var x = checkedRows.indexOf(pageRowIds[i]);
		if (is_checked == true)
		{
		  page_checked = true;
		  if (x == -1)
    	      checkedRows[n++] = pageRowIds[i];
		}
        else if (x != -1) 
            checkedRows.splice(x);
	}
 	var checkedRowsStr = JSON.stringify(checkedRows);
    
	var up_down = $('#up_down').val();
	$('#up_down').val(3 - parseInt(up_down));
	
	var tri_cls = '';
	if (up_down == '2')
		tri_cls = 'arrow-up-color';
	else
		tri_cls = 'arrow-down-color';

    if (page_checked)
    {
      $('#chk_page').attr('checked', 'checked');
    }
    else
    {
        $('#chk_page').removeAttr('checked');
    }
	$('#head1').html('<a href="javascript:void(0);" onclick="openFilter(\'1\');">Date</a>&nbsp;&nbsp;<div class="arrow-up" onclick="sort(\'1\');"></div>');
	$('#head2').html('<a href="javascript:void(0);">HeadLine</a>&nbsp;&nbsp;<div class="arrow-up" onclick="sort(\'2\');"></div>');
	$('#head3').html('<a href="javascript:void(0);" onclick="openFilter(\'3\');">Source</a>&nbsp;&nbsp;<div class="arrow-up" onclick="sort(\'3\');"></div>');
	$('#head4').html('<a href="javascript:void(0);" onclick="openFilter(\'4\');">Event</a>&nbsp;&nbsp;<div class="arrow-up" onclick="sort(\'4\');"></div>');
	$('#head5').html('<a href="javascript:void(0);" onclick="openFilter(\'5\');">Country</a>&nbsp;&nbsp;<div class="arrow-up" onclick="sort(\'5\');"></div>');
	$('#head6').html('<a href="javascript:void(0);" onclick="openFilter(\'6\');">Sentiment</a>&nbsp;&nbsp;<div class="arrow-up" onclick="sort(\'6\');"></div>');
    
	if (val == '1')
		$('#head' + val).html('<span style="font-weight:bold; color:#00A2E8"><a href="javascript:void(0);" onclick="openFilter(\'1\');">Date</a></span>&nbsp;&nbsp;<div class="' + tri_cls + '" onclick="sort(\'1\');"></div>');
	else if (val == '2')
		$('#head' + val).html('<span style="font-weight:bold; color:#00A2E8"><a href="javascript:void(0);">HeadLine</a></span>&nbsp;&nbsp;<div class="' + tri_cls + '" onclick="sort(\'2\');"></div>');
	else if (val == '3')
		$('#head' + val).html('<span style="font-weight:bold; color:#00A2E8"><a href="javascript:void(0);" onclick="openFilter(\'3\');">Source</a></span>&nbsp;&nbsp;<div class="' + tri_cls + '" onclick="sort(\'3\');"></div>');
	else if (val == '4')
		$('#head' + val).html('<span style="font-weight:bold; color:#00A2E8"><a href="javascript:void(0);" onclick="openFilter(\'4\');">Event</a></span>&nbsp;&nbsp;<div class="' + tri_cls + '" onclick="sort(\'4\');"></div></a>');
	else if (val == '5')
		$('#head' + val).html('<span style="font-weight:bold; color:#00A2E8"><a href="javascript:void(0);" onclick="openFilter(\'5\');">Country</a></span>&nbsp;&nbsp;<div class="' + tri_cls + '" onclick="sort(\'5\');"></div></a>');
	else if (val == '6')
		$('#head' + val).html('<span style="font-weight:bold; color:#00A2E8"><a href="javascript:void(0);" onclick="openFilter(\'6\');">Sentiment</a></span>&nbsp;&nbsp;<div class="' + tri_cls + '" onclick="sort(\'6\');"></div></a>');

	$('#sort').val(val);
	$('#cur_pg').val('1');
	var keyword = $('#keyword').val();
	$('#pg_Nav').html('<tr style="line-height:39px;"><td>&nbsp;</td></tr>');
	$('#pg_Nav1').html('<tr style="line-height:39px;"><td>&nbsp;</td></tr>');
	$('#tbody_content').html('<tr class="output4 theStylingClass" style="cursor:pointer;"><td colspan="6" align="center"><img src="img/loading.gif"></td></tr>');
    
	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'1',keyword:keyword, page:'1', sort:val, up_down:up_down, checkedRows:checkedRowsStr, option1:optStr1, option2:optStr2, option3:optStr3, option4:optStr4, optionKind:optionKind, industry1:industry1, industry2:industry2},
		success : function(data){
			console.log(data);
			processJson(data);
		}
	});
	return false;
}

function openFilter(val)
{
	if (val == '1') // DATE
	{
		$('#drop_down_list').html('');
		var news_date_filter1 = jQuery.parseJSON($('#dt_filter_str1').val());
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str').val());

		var html1 = '<ul class="sb_dropdown" style="display:none; position:absolute" id="dt_filter"><li class="sb_filter">Filter your search</li>';
		html1 += '<li><input type="checkbox" id="chk_all1" onclick="chk_all(\'1\');"/><label for="chk_all1">Check All</label></li><li></li><li></li>';

		var flag = '1';
		for (i = 0; i < news_date_filter.length; i++)
		{
			var indd = jQuery.inArray( news_date_filter[i], news_date_filter1 );
			if (indd == '-1')
			{
				flag = '2';
				html1 += '<li style="width:33%"><input type="checkbox" onclick="chk_each(\'1\');" id="chk_1_' + i + '"/><label for="chk_1_' + i + '">' + news_date_filter[i] + '</label></li>'; 
			}
			else
				html1 += '<li style="width:33%"><input type="checkbox" onclick="chk_each(\'1\');" checked id="chk_1_' + i + '"/><label for="chk_1_' + i + '">' + news_date_filter[i] + '</label></li>'; 
		}
		html1 += '<li class="sb_filter1" style="width:348px;height:25px; line-height:25px; padding:5px; text-align:center;"><input type="button" name="confirm" value="Confirm" onclick="confirm(\'1\');" style="text-align:center">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type="button" name="cancel" value="Cancel" onclick="cancel();" style="text-align:center"></li>';
		html1 += '</ul>';

		$('#drop_down_list').html(html1);

		if (flag == '1')
			$('#chk_all1').attr('checked', 'checked');
	}
	else if (val == '3') // Company
	{
		$('#drop_down_list').html('');
		var news_source_filter1 = jQuery.parseJSON($('#source_filter_str1').val());
		var news_source_filter = jQuery.parseJSON($('#source_filter_str').val());

		var html1 = '<ul class="sb_dropdown" style="display:none; position:absolute; width:600px" id="dt_filter"><li class="sb_filter" style="width:590px;">Filter your search</li>';
		html1 += '<li><input type="checkbox" id="chk_all2" onclick="chk_all(\'2\');"/><label for="chk_all2">Check All</label></li><li></li>';

		var flag = '1';
		for (i = 0; i < news_source_filter.length; i++)
		{
			var indd = jQuery.inArray( news_source_filter[i], news_source_filter1 );
			if (indd == '-1')
			{
				flag = '2';
				html1 += '<li style="width:50%"><input type="checkbox" onclick="chk_each(\'2\');" id="chk_2_' + i + '"/><label for="chk_2_' + i + '">' + news_source_filter[i] + '</label></li>'; 
			}
			else
				html1 += '<li style="width:50%"><input type="checkbox" onclick="chk_each(\'2\');" checked id="chk_2_' + i + '"/><label for="chk_2_' + i + '">' + news_source_filter[i] + '</label></li>'; 
		}
		html1 += '<li class="sb_filter1" style="width:590px;height:25px; line-height:25px; padding:5px; text-align:center;"><input type="button" name="confirm" value="Confirm" onclick="confirm(\'3\');" style="text-align:center">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type="button" name="cancel" value="Cancel" onclick="cancel();" style="text-align:center"></li>';
		html1 += '</ul>';

		$('#drop_down_list').html(html1);

		if (flag == '1')
			$('#chk_all2').attr('checked', 'checked');
	}
	else if (val == '4') // EVENT
	{
		$('#drop_down_list').html('');
		var news_event_filter1 = jQuery.parseJSON($('#event_filter_str1').val());
		var news_event_filter = jQuery.parseJSON($('#event_filter_str').val());

		var html1 = '<ul class="sb_dropdown" style="display:none; position:absolute" id="dt_filter"><li class="sb_filter">Filter your search</li>';
		html1 += '<li><input type="checkbox" id="chk_all3" onclick="chk_all(\'3\');"/><label for="chk_all3">Check All</label></li><li></li>';

		var flag = '1';
		for (i = 0; i < news_event_filter.length; i++)
		{
			var indd = jQuery.inArray( news_event_filter[i], news_event_filter1 );
			if (indd == '-1')
			{
				flag = '2';
				html1 += '<li style="width:50%"><input type="checkbox" onclick="chk_each(\'3\');" id="chk_3_' + i + '"/><label for="chk_3_' + i + '">' + news_event_filter[i] + '</label></li>'; 
			}
			else
				html1 += '<li style="width:50%"><input type="checkbox" onclick="chk_each(\'3\');" checked id="chk_3_' + i + '"/><label for="chk_3_' + i + '">' + news_event_filter[i] + '</label></li>'; 
		}
		html1 += '<li class="sb_filter1" style="width:348px;height:25px; line-height:25px; padding:5px; text-align:center;"><input type="button" name="confirm" value="Confirm" onclick="confirm(\'4\');" style="text-align:center">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type="button" name="cancel" value="Cancel" onclick="cancel();" style="text-align:center"></li>';
		html1 += '</ul>';

		$('#drop_down_list').html(html1);

		if (flag == '1')
			$('#chk_all3').attr('checked', 'checked');
	}
	else if (val == '5') // Country
	{
		$('#drop_down_list').html('');
		var news_country_filter1 = jQuery.parseJSON($('#country_filter_str1').val());
		var news_country_filter = jQuery.parseJSON($('#country_filter_str').val());

		var html1 = '<ul class="sb_dropdown" style="display:none; position:absolute" id="dt_filter"><li class="sb_filter">Filter your search</li>';
		html1 += '<li><input type="checkbox" id="chk_all4" onclick="chk_all(\'4\');"/><label for="chk_all4">Check All</label></li><li></li>';

		var flag = '1';
		for (i = 0; i < news_country_filter.length; i++)
		{
			var indd = jQuery.inArray( news_country_filter[i], news_country_filter1 );
			if (indd == '-1')
			{
				flag = '2';
				html1 += '<li style="width:50%"><input type="checkbox" onclick="chk_each(\'4\');" id="chk_4_' + i + '"/><label for="chk_4_' + i + '">' + news_country_filter[i] + '</label></li>'; 
			}
			else
				html1 += '<li style="width:50%"><input type="checkbox" onclick="chk_each(\'4\');" checked id="chk_4_' + i + '"/><label for="chk_4_' + i + '">' + news_country_filter[i] + '</label></li>'; 
		}
		html1 += '<li class="sb_filter1" style="width:348px;height:25px; line-height:25px; padding:5px; text-align:center;"><input type="button" name="confirm" value="Confirm" onclick="confirm(\'5\');" style="text-align:center">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type="button" name="cancel" value="Cancel" onclick="cancel();" style="text-align:center"></li>';
		html1 += '</ul>';

		$('#drop_down_list').html(html1);

		if (flag == '1')
			$('#chk_all4').attr('checked', 'checked');
	}
    else if (val == '6') // Sentiment
	{
	   return;
	}
	var x = $('#head' + val).offset();
	console.log(x);
	//$('.sb_dropdown').offset({ top: x.top, left: x.left });
	$('.sb_dropdown').css({
		position: 'absolute',
		top: x.top + 40,
		left: x.left - 10
	});
	$('.sb_dropdown').show();
}

function chk_each(val)
{
	var flag = '1';
	var obj = $('input[id^="chk_' + val + '_"]');
	for (i = 0; i < obj.length; i++)
	{
		var chk = $(obj[i]).is(':checked');
		if (chk == false)
		{
			flag = '2';
			break;
		}
	}
	console.log(flag);

	if (flag == '1') // CHECK ALL
	{
		$('#chk_all' + val).attr('checked', 'checked');
	}
	else if (flag == '2') // UNCHECK ALL
	{
		$('#chk_all' + val).removeAttr('checked');
	}
}

function chk_all(val)
{
	var chk = $('#chk_all' + val).is(':checked');
	if (chk == true)
	{
		var obj = $('input[id^="chk_' + val + '_"]');
		for (i = 0; i < obj.length; i++)
		{
			$(obj[i]).attr('checked', 'checked');
		}
	}
	else
	{
		var obj = $('input[id^="chk_' + val + '_"]');
		for (i = 0; i < obj.length; i++)
		{
			$(obj[i]).removeAttr('checked');
		}
	}
}

function check_page()
{
    var pageRowIds = jQuery.parseJSON($('#pageRowIds').val());
    var chk = $('#chk_page').is(':checked');
    if (chk == true)
    {
        for (i = 0; i < pageRowIds.length; i++)
        {
            $('#chk_5_' + pageRowIds[i]).attr('checked', 'checked');
        }
    }
    else
    {
        for (i = 0; i < pageRowIds.length; i++)
        {
            $('#chk_5_' + pageRowIds[i]).removeAttr('checked');
        }
    }
}

function confirm(val1)
{
	var opt_chk1 = [];
	var opt_chk2 = [];
	var opt_chk3 = [];
	var opt_chk4 = [];
	var ind = 0;
	if (val1 == '1')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			var val = $('#chk_1_' + i).is(':checked');
			if (val == true)
			{
				opt_chk1[ind++] = $('label[for="chk_1_' + i + '"]').text();
			}
		}

		ind = 0;
		var news_source_filter = jQuery.parseJSON($('#source_filter_str1').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			opt_chk2[i] = news_source_filter[i];
		}

		var news_event_filter = jQuery.parseJSON($('#event_filter_str1').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			opt_chk3[i] = news_event_filter[i];
		}

		var news_country_filter = jQuery.parseJSON($('#country_filter_str1').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			opt_chk4[i] = news_country_filter[i];
		}

		if (opt_chk1.length == 0)
		{
			alert('Please check at least one option');
			return;
		}
	}
	else if (val1 == '3')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str1').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			opt_chk1[i] = news_date_filter[i];
		}
		
		var news_source_filter = jQuery.parseJSON($('#source_filter_str').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			var val = $('#chk_2_' + i).is(':checked');
			if (val == true)
			{
				opt_chk2[ind++] = $('label[for="chk_2_' + i + '"]').text();
			}
		}

		var news_event_filter = jQuery.parseJSON($('#event_filter_str1').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			opt_chk3[i] = news_event_filter[i];
		}

		var news_country_filter = jQuery.parseJSON($('#country_filter_str1').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			opt_chk4[i] = news_country_filter[i];
		}

		if (opt_chk2.length == 0)
		{
			alert('Please check at least one option');
			return;
		}
	}
	else if (val1 == '4')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str1').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			opt_chk1[i] = news_date_filter[i];
		}

		ind = 0;
		var news_source_filter = jQuery.parseJSON($('#source_filter_str1').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			opt_chk2[i] = news_source_filter[i];
		}
		
		var news_event_filter = jQuery.parseJSON($('#event_filter_str').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			var val = $('#chk_3_' + i).is(':checked');
			if (val == true)
			{
				opt_chk3[ind++] = $('label[for="chk_3_' + i + '"]').text();
			}
		}
		if (opt_chk3.length == 0)
		{
			alert('Please check at least one option');
			return;
		}

		var news_country_filter = jQuery.parseJSON($('#country_filter_str1').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			opt_chk4[i] = news_country_filter[i];
		}
	}
	else if (val1 == '5')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str1').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			opt_chk1[i] = news_date_filter[i];
		}

		var news_source_filter = jQuery.parseJSON($('#source_filter_str1').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			opt_chk2[i] = news_source_filter[i];
		}

		var news_event_filter = jQuery.parseJSON($('#event_filter_str1').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			opt_chk3[i] = news_event_filter[i];
		}

		ind = 0;
		var news_country_filter = jQuery.parseJSON($('#country_filter_str').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			var val = $('#chk_4_' + i).is(':checked');
			if (val == true)
			{
				opt_chk4[ind++] = $('label[for="chk_4_' + i + '"]').text();
			}
		}
		if (opt_chk4.length == 0)
		{
			alert('Please check at least one option');
			return;
		}
	}

	var col_ind = $('#col_ind').val();
	if (col_ind != val)
	{
		up_down = '2';
		$('#up_down').val('1');
		$('#col_ind').val(val1);
	}

	var optStr1 = JSON.stringify(opt_chk1);
	var optStr2 = JSON.stringify(opt_chk2);
	var optStr3 = JSON.stringify(opt_chk3);
	var optStr4 = JSON.stringify(opt_chk4);

	Submit_Option(optStr1, optStr2, optStr3, optStr4, val1);
}

function Submit_Option(optStr1, optStr2, optStr3, optStr4, val)
{
	var industry1 = 1, industry2 = 1;
//	if ($('#chk_industry1').is(':checked') == false)
//		industry1 = 0;
//	if ($('#chk_industry2').is(':checked') == false)
//		industry2 = 0;

	$('.sb_dropdown').hide();
	var sort = $('#sort').val();
	$('#cur_pg').val('1');
	var keyword = $('#keyword').val();
	var up_down = 3 - parseInt($('#up_down').val());

	$('#tbody_content').html('<tr class="output4 theStylingClass" style="cursor:pointer;"><td colspan="6" align="center"><img src="img/loading.gif"></td></tr>');
    
       // checked row filter
    var checkedRows = [];
   	var checkedRows1 = jQuery.parseJSON($('#checkedRows').val());
    var n = 0;
    if (checkedRows1 != null)
    {
       n = checkedRows1.length;
	   for (i = 0; i < n; i++)
	     checkedRows[i] = checkedRows1[i];
    }
  	var pageRowIds = jQuery.parseJSON($('#pageRowIds').val());
 	for (i = 0; i < pageRowIds.length; i++)
	{
		var val = $('#chk_5_' + pageRowIds[i]).is(':checked');
        var x = checkedRows.indexOf(pageRowIds[i]);
		if (val == true)
		{
		  if (x == -1)
    	      checkedRows[n++] = pageRowIds[i];
		}
        else if (x != -1) 
            checkedRows.splice(x);
	}
 	var checkedRowsStr = JSON.stringify(checkedRows);
    
	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'1',keyword:keyword, page:'1', sort:sort, up_down:up_down, checkedRows:checkedRowsStr, option1:optStr1, option2:optStr2, option3:optStr3, option4:optStr4, optionKind:'4', industry1:industry1, industry2:industry2},
		success : function(data){
			console.log(data);
			processJson(data);
		}
	});
}

function cancel()
{
	$('.sb_dropdown').hide();
}

function add_column()
{
    document.getElementById('delete_button').style.display = 'none';
    loading(); // loading
    loadPopup();
    $('#popup_title').text('Add Column');
    $('#row_id').val(0);
    $('#item_id').val(0);
    $('#column_id').val(0);
    $('#popup_text').val('');
}

function add_column_data(rowId, columnId)
{
    document.getElementById('delete_button').style.display = 'none';
    loading(); 
    loadPopup();
    $('#popup_title').text('Add Column data');
    $('#column_id').val(columnId);
    $('#row_id').val(rowId);
    $('#item_id').val(0);
    $('#popup_text').val('');
}

function change_column_data(itemId)
{
    loading(); 
	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'6', id:itemId},
		success : function(data){
			setTimeout(function(){ // then show popup, deley in .5 second
				loadPopup(); // function show popup
			}, 500); // .5 second
             document.getElementById('delete_button').style.display = 'inline';
     		$('#popup_title').text('Edit Column data');
            $('#item_id').val(itemId);
            $('#popup_text').val(data);
            $('#row_id').val(1);
            $('#column_id').val(1);
 		}
	});
}

function openUserColumn(val)
{
    loading(); 
 	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'4'},
		success : function(data){
			setTimeout(function(){ // then show popup, deley in .5 second
				loadPopup(); // function show popup
			}, 500); // .5 second
            var obj = jQuery.parseJSON(data);
            document.getElementById('delete_button').style.display = 'inline';
    		$('#popup_title').text('Edit Column');
            $('#column_id').val(obj.id);
            $('#popup_text').val(obj.column_name);
            $('#row_id').val(0);
            $('#item_id').val(0);
		}
  	});
}
    
function submit_popup()
{
	var rowId = $('#row_id').val();
    var columnId = $('#column_id').val();
    var item_id = $('#item_id').val();
	var txt = $('#popup_text').val();
    if (rowId == 0) // add column
    {
	   if (txt == "")
	   {
	      alert('Enter column name.');
		  return;
	   }
	   $.ajax({
		  type: "POST", 
		  url: "index.php",
		  data : {postflag:'5',column_name:txt, id:columnId},
		  success : function(data){
            disablePopup();
            pg_Navigate();
		  }
	   });
    }
    else
    {
	   if (txt == "")
	   {
	      alert('Enter column data text.');
		  return;
	   }
	   $.ajax({
		  type: "POST", 
		  url: "index.php",
		  data : {postflag:'7',data_text:txt, id:item_id, news_id:rowId, column_id:columnId},
		  success : function(data){
            disablePopup();
            pg_Navigate();
		  }
	   });
    }
}

function delete_popup()
{
	var rowId = $('#row_id').val();
    var columnId = $('#column_id').val();
    var item_id = $('#item_id').val();
	var txt = $('#popup_text').val();
    if (rowId == 0) // delete column
    {
	   $.ajax({
		  type: "POST", 
		  url: "index.php",
		  data : {postflag:'8', id:columnId},
		  success : function(data){
            disablePopup();
            pg_Navigate();
		  }
	   });
    }
    else
    {
	   $.ajax({
		  type: "POST", 
		  url: "index.php",
		  data : {postflag:'9',id:item_id},
		  success : function(data){
            disablePopup();
            pg_Navigate();
		  }
	   });
    }
}

function export_excel()
{
 	var val1 = $('#col_ind').val();
	var opt_chk1 = [];
	var opt_chk2 = [];
	var opt_chk3 = [];
	var opt_chk4 = [];
	var ind = 0;
	if (val1 == '1')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str1').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			opt_chk1[i] = news_date_filter[i];
		}

		var news_source_filter = jQuery.parseJSON($('#source_filter_str').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			opt_chk2[i] = news_source_filter[i];
		}

		var news_event_filter = jQuery.parseJSON($('#event_filter_str').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			opt_chk3[i] = news_event_filter[i];
		}

		var news_country_filter = jQuery.parseJSON($('#country_filter_str').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			opt_chk4[i] = news_country_filter[i];
		}
	}
	else if (val1 == '3')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			opt_chk1[i] = news_date_filter[i];
		}
		
		var news_source_filter = jQuery.parseJSON($('#source_filter_str1').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			opt_chk2[i] = news_source_filter[i];
		}

		var news_event_filter = jQuery.parseJSON($('#event_filter_str').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			opt_chk3[i] = news_event_filter[i];
		}

		var news_country_filter = jQuery.parseJSON($('#country_filter_str').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			opt_chk4[i] = news_country_filter[i];
		}
	}
	else if (val1 == '4')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			opt_chk1[i] = news_date_filter[i];
		}

		var news_source_filter = jQuery.parseJSON($('#source_filter_str').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			opt_chk2[i] = news_source_filter[i];
		}
		
		var news_event_filter = jQuery.parseJSON($('#event_filter_str1').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			opt_chk3[i] = news_event_filter[i];
		}

		var news_country_filter = jQuery.parseJSON($('#country_filter_str').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			opt_chk4[i] = news_country_filter[i];
		}
	}
	else if (val1 == '5')
	{
		var news_date_filter = jQuery.parseJSON($('#dt_filter_str').val());
		for (i = 0; i < news_date_filter.length; i++)
		{
			opt_chk1[i] = news_date_filter[i];
		}

		var news_source_filter = jQuery.parseJSON($('#source_filter_str').val());
		for (i = 0; i < news_source_filter.length; i++)
		{
			opt_chk2[i] = news_source_filter[i];
		}

		var news_event_filter = jQuery.parseJSON($('#event_filter_str').val());
		for (i = 0; i < news_event_filter.length; i++)
		{
			opt_chk3[i] = news_event_filter[i];
		}

		var news_country_filter = jQuery.parseJSON($('#country_filter_str').val());
		for (i = 0; i < news_country_filter.length; i++)
		{
			opt_chk4[i] = news_country_filter[i];
		}
	}

	var optStr1 = JSON.stringify(opt_chk1);
	var optStr2 = JSON.stringify(opt_chk2);
	var optStr3 = JSON.stringify(opt_chk3);
	var optStr4 = JSON.stringify(opt_chk4);

	var industry1 = 1, industry2 = 1;
//	if ($('#chk_industry1').is(':checked') == false)
//		industry1 = 0;
//	if ($('#chk_industry2').is(':checked') == false)
//		industry2 = 0;

	var sort = $('#sort').val();
	var keyword = $('#keyword').val();
	var up_down = 3 - parseInt($('#up_down').val());
    
    // checked row filter
    var checkedRows = [];
   	var checkedRows1 = jQuery.parseJSON($('#checkedRows').val());
    var n = 0;
    if (checkedRows1 != null)
    {
       n = checkedRows1.length;
	   for (i = 0; i < n; i++)
	     checkedRows[i] = checkedRows1[i];
    }
  	var pageRowIds = jQuery.parseJSON($('#pageRowIds').val());
 	for (i = 0; i < pageRowIds.length; i++)
	{
		var val = $('#chk_5_' + pageRowIds[i]).is(':checked');
        var x = checkedRows.indexOf(pageRowIds[i]);
		if (val == true)
		{
		  if (x == -1)
    	      checkedRows[n++] = pageRowIds[i];
		}
        else if (x != -1) 
            checkedRows.splice(x);
	}
 	var checkedRowsStr = JSON.stringify(checkedRows);
    
	$.ajax({
		type: "POST", 
		url: "index.php",
		data : {postflag:'3',keyword:keyword, sort:sort, up_down:up_down, checkedRows:checkedRowsStr, option1:optStr1, option2:optStr2, option3:optStr3, option4:optStr4, optionKind:'4', industry1:industry1, industry2:industry2},
		success : function(data){
			console.log(data);
			$('#sql').val(data);
			$('[name=mainForm]').submit();
		}
	});
}
</script>
    

<link rel="stylesheet" type="text/css" href="//ct1.addthis.com/static/r07/widget/css/widget007.old.css" media="all"></head>

<body><a id="BackToTop" href="#body" style="display: none;"><span>^ Back to top</span></a>

	<div class="theBody">

		<?php include('header.php'); ?>

		<div class="centralize">
			<div class="page_container">		

				<div class="middle_column" style="width:850px; min-width:850px;">
					<!--<div class="shadow_top"></div>-->
					<div id="home_page" class="middle_container" style="width:100%">
						
						<div class="middle" style="width:100%; min-width:850px;">
							<p>&nbsp;</p>
						
							<div class="clear"></div>

							<input type="hidden" name="itemNum" id="itemNum" value>
							<input type="hidden" name="pgNum" id="pgNum" value>
							<input type="hidden" name="cur_pg" id="cur_pg" value="-1">
							<input type="hidden" name="sort" id="sort" value="1">
							<input type="hidden" name="up_down" id="up_down" value="2">
							<input type="hidden" name="col_ind" id="col_ind" value="1">

							<input type="hidden" name="dt_filter_str" id="dt_filter_str" value="">
							<input type="hidden" name="source_filter_str" id="source_filter_str" value="">
							<input type="hidden" name="dt_filter_str1" id="dt_filter_str1" value="">
							<input type="hidden" name="source_filter_str1" id="source_filter_str1" value="">
							<input type="hidden" name="event_filter_str" id="event_filter_str" value="">
							<input type="hidden" name="event_filter_str1" id="event_filter_str1" value="">
							<input type="hidden" name="country_filter_str" id="country_filter_str" value="">
							<input type="hidden" name="country_filter_str1" id="country_filter_str1" value="">
                            <input type="hidden" name="pageRowIds" id="pageRowIds" value="">
                            <input type="hidden" name="checkedRows" id="checkedRows" value="">
                            <input type="hidden" name="userColumns" id="userColumns" value="">
							<form name="mainForm" method="post" action="export.php">
								<input type="hidden" name="sql" id="sql" value="">
							</form>

							<div class="carousel_content">
								<div class="carousel" style="width:100%; margin-left:-10px;">

									<form class="form-wrapper cf" onsubmit="return false;">
										<input type="text" placeholder="Search for a specific news : " id="keyword">
										<button onclick="return check();">Search</button>
									</form>

									<!--
                                    <div style="margin-top:0px; width:100%; text-align:center;">
										<input type="checkbox" name="chk_industry" id="chk_industry1" style="margin-top:-2px;" checked>
										<label for="chk_industry1" style="display:inline; font-size:15px">&nbsp;&nbsp;RE companies</label>
										&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
										<input type="checkbox" name="chk_industry" id="chk_industry2" style="margin-top:-2px;">
										<label for="chk_industry2" style="display:inline; font-size:15px">&nbsp;&nbsp;Publications</label>
									</div>
									-->
                                    <!-- <div id="chart"></div>   168px -> 488 -->
									<div class="clear"></div>
									<div style="margin-top:10px; width:100%">
										<table width="100%" cellpadding="10" id="pg_Nav" cellspacing="0" style=" font-size:15px;">
										</table>
										<table width="100%" cellpadding="10" id="information" cellspacing="0" style="border:2px solid lightgray; font-size:15px; display:none">
											<thead>
												<tr style="font-size:16px; color:#636363;background:#C6C6C6;width:80%;">
													<th class="bsorttable_nosort" style="width:18, weight:10;"><input type="checkbox" name="chk_page" id="chk_page" onclick="check_page();"></div></th>
													<th class="bsorttable_nosort" style="width:13%; cursor:pointer;" id="head1"><a href="javascript:void(0);" onclick="openFilter('1');">Date</a>&nbsp;&nbsp;<!--<a href="javascript:void(0);" class="main_link" onclick="openFilter('1');"></a>-->
													<div class="arrow-up" onclick="sort('1');"></div>
													</th>
													<th class="bsorttable_nosort" style="width:34%; cursor:pointer;" id="head2"><a href="javascript:void(0);">HeadLine</a>&nbsp;&nbsp;<div class="arrow-up" onclick="sort('2');"></div></th>
													<th class="bsorttable_nosort1" style="width:16%; cursor:pointer;" id="head3"><a href="javascript:void(0);" onclick="openFilter('3');">Source</a>&nbsp;&nbsp;<!--<a href="javascript:void(0);" class="main_link1" onclick="openFilter('3');"></a>-->
													<div class="arrow-up" onclick="sort('3');"></div>
													</th>
													<th class="bsorttable_nosort1" style="width:10%; cursor:pointer;" id="head4"><a href="javascript:void(0);" onclick="openFilter('4');">Event</a>&nbsp;&nbsp;<!--<a href="javascript:void(0);" class="main_link2" onclick="openFilter('4');"></a>-->
													<div class="arrow-up" onclick="sort('4');"></div>
													<th class="bsorttable_nosort1" style="width:12%; cursor:pointer;" id="head5"><a href="javascript:void(0);" onclick="openFilter('5');">Country</a>&nbsp;&nbsp;<!--<a href="javascript:void(0);" class="main_link2" onclick="openFilter('4');"></a>-->
													<div class="arrow-up" onclick="sort('5');"></div>
													</th>
                                                    <th class="bsorttable_nosort1" style="width:14%; cursor:pointer;" id="head6"><a href="javascript:void(0);" onclick="openFilter('6');">Sentiment</a>&nbsp;&nbsp;<!--<a href="javascript:void(0);" class="main_link2" onclick="openFilter('4');"></a>-->
													<div class="arrow-up" onclick="sort('6');"></div>
													</th>
                                                    <th class="bsorttable_nosort1" style="display:none; width:10%; cursor:pointer;" id="head7"><a href="javascript:void(0);" onclick="openUserColumn('1');">User column</a>&nbsp;&nbsp;<!--<a href="javascript:void(0);" class="main_link2" onclick="openFilter('4');"></a>-->
													<!-- <div class="arrow-up" onclick="sort('7');"></div> -->
													</th>
												</tr>
											</thead>
											<tbody id="tbody_content">
											</tbody>
										</table>
										<table width="100%" cellpadding="10" id="pg_Nav1" cellspacing="0" style=" font-size:15px;">
										</table>
									</div>
								</div>
							</div>
						</div>
						<div class="clear"></div>
					</div>

					<!--<div class="shadow_bottom"></div>-->
					<div class="footer" style="width:100%">
						<div class="footer-bg">				
							<ul>
								©2015 Kensee Ltd. All rights reserved.
							</ul>
							<div class="clear"></div>
						</div>
					</div>
				</div>

				<div class="detailPanel" style="width:330px; float:left; background-color:white; border: 2px solid lightgray; border-top-left-radius: 15px; border-top-right-radius: 15px; box-shadow: 5px 5px 5px lightgray; background-color:white; padding:15px 10px 15px 10px; margin-top:5px; margin-left:10px; height:1000px; display:none">
				</div>
			</div>

			<div id="drop_down_list">
			<ul class="sb_dropdown" style="display:none; position:absolute">
				<li class="sb_filter">Filter your search</li>
				<li><input type="checkbox"/><label for="all"><strong>All Categories</strong></label></li>
				<li><input type="checkbox"/><label for="Automotive">Automotive</label></li>
				<li><input type="checkbox"/><label for="Baby">Baby</label></li>
				<li><input type="checkbox"/><label for="Beauty">Beautys</label></li>
				<li><input type="checkbox"/><label for="Books">Books</label></li>
				<li><input type="checkbox"/><label for="Cell">Cell Phones &amp; Service</label></li>
				<li><input type="checkbox"/><label for="Cloth">Clothing &amp; Accessories</label></li>
				<li><input type="checkbox"/><label for="Electronics">Electronics</label></li>
				<li><input type="checkbox"/><label for="Gourmet">Gourmet Food</label></li>
				<li><input type="checkbox"/><label for="Health">Health &amp; Personal Care</label></li>
				<li><input type="checkbox"/><label for="Home">Home &amp; Garden</label></li>
				<li><input type="checkbox"/><label for="Industrial">Industrial &amp; Scientific</label></li>
				<li><input type="checkbox"/><label for="Jewelry">Jewelry</label></li>
				<li><input type="checkbox"/><label for="Magazines">Magazines</label></li>
				<input type="button" name="confirm" value="Confirm" style="text-align:center">
				<input type="button" name="cancel" value="Cancel" style="text-align:center">
			</ul>
			</div>
			
			<div class="clear"></div>

			<div id="toPopup">        
				<div class="close"></div>
				<span class="ecs_tooltip">Press Esc to close <span class="arrow"></span></span>
				<div id="popup_content" style="text-align:center;"> <!--your content start-->
					<table width="100%" cellpadding="10" cellspacing="10" border='0' style='table-layout: fixed;'>
						<tr>
							<input type="hidden" name="column_id" id="column_id">
                            <input type="hidden" name="row_id" id="row_id">
                            <input type="hidden" name="item_id" id="item_id">
							<td align="center"><BR><h2 align="center" id="popup_title" name="popup_title">Add</h2></td>
						</tr>
						<tr>
							<td>
								<textarea id="popup_text" name="popup_text" cols="6" rows="40" style="height:150px; width:485px; line-height:25px;"></textarea>
							</td>
						</tr>
						<tr>
							<td align="center">
								<a href="javascript:void(0);" class="ph-button ph-btn-blue" style="display:inline;" onclick="submit_popup();"><span style="color:#ffffff; font-size:14px;">Submit</span></a>
                                <b id="delete_button" href="javascript:void(0);" class="ph-button ph-btn-blue" style="display:none;" onclick="delete_popup();"><span style="color:#ffffff; font-size:14px;">Delete</span></b>
							</td>
						</tr>
					</table>
				</div> <!--your content end-->
			</div> <!--toPopup end-->
			<div class="loader">
			</div>
			<div id="backgroundPopup">
			</div>
		</div>
	</div>

	<div id="modal" class="modal hide fade" tabindex="-1" role="dialog" aria-hidden="true">
		<div style="padding:10px; text-align:center;">
			<img src="<?php echo $SITEURL; ?>/img/loading.gif"> Loading...
		</div>
	</div>
	
	<div id="generic_modal" class="modal hide fade" tabindex="-1" role="dialog" aria-hidden="true">
		<div style="padding:10px; text-align:center;">
			<img src="<?php echo $SITEURL; ?>/img/loading.gif"> Loading...
		</div>
	</div>
					
	<script type="text/javascript" src="http://s7.addthis.com/js/300/addthis_widget.js#pubid=ra-50328c8b26ac1070"></script><div id="_atssh" style="visibility: hidden; height: 1px; width: 1px; position: absolute; z-index: 100000;"><iframe id="_atssh138" title="AddThis utility frame" src="//ct1.addthis.com/static/r07/sh180.html#iit=1418111560989&amp;tmr=load%3D1418111557832%26core%3D1418111558088%26main%3D1418111560981%26ifr%3D1418111561078&amp;cb=0&amp;cdn=1&amp;kw=oo.sg%2Cbook%20price%20comparison%2Csingapore%20book%20prices%2Csingapore%20bestselling%20books%2Csingapore%20bestsellers%2Cbest%20book%20prices%2Csingapore%20online%20bookstores%2Csingapore%20book%20promotions%2Camazon%2Ckinokuniya&amp;ab=-&amp;dh=oo.sg&amp;dr=&amp;du=http%3A%2F%2Foo.sg%2F&amp;dt=OO.sg%20-%20Compare%20Book%20Prices%2C%20Buy%20Books%20Online%20from%20Singapore%20Bookstores&amp;dbg=0&amp;cap=tc%3D0%26ab%3D0&amp;inst=1&amp;jsl=33&amp;prod=undefined&amp;lng=en-GB&amp;ogt=&amp;pc=men&amp;pub=ra-50328c8b26ac1070&amp;ssl=0&amp;sid=5486aa465bb0f186&amp;srpl=1&amp;srcs=1&amp;srd=1&amp;srf=1&amp;srx=1&amp;ver=300&amp;xck=0&amp;xtr=0&amp;og=&amp;aa=0&amp;csi=undefined&amp;toLoJson=uvs%3D5486aa46ac0b295e000%26chr%3DUTF-8%26md%3D0%26vcl%3D1&amp;rev=10.2&amp;ct=1&amp;xld=1&amp;xd=1" style="height: 1px; width: 1px; position: absolute; z-index: 100000; border: 0px; left: 0px; top: 0px;"></iframe></div><script type="text/javascript" src="http://ct1.addthis.com/static/r07/core165.js"></script>
	<script type="text/javascript">
	$(function() {
	
		$('#at-gsm').mouseover(function() {
			$(this).data('is_hovered', true);
			window.setTimeout(function() {
				if ($('#at-gsm').data('is_hovered')) {
					$('#at-gsm ul').slideDown('fast');
				}
			}, 500); // 1/2 second delay
		});
	
		$('#at-gsm').mouseout(function() {
			$(this).data('is_hovered', false);
			if ($(this).data('timeout')) {
				window.clearTimeout($(this).data('timeout'));
			}
			$(this).data('timeout', window.setTimeout(function() {
				if (!$('#at-gsm').data('is_hovered')) {
					$('#at-gsm ul').slideUp('fast');
				}
			}, 500)); // 1/2 second delay
		});
		
	});
	</script>
	<script type="text/javascript">
	$(document).ready(function(){
		BackToTop({
				text : '^ Back to top',
				autoShow : true,
				timeEffect : 800,
				appearMethod : 'slide',
				effectScroll : 'easeOutCubic' /** all effects http://jqueryui.com/docs/effect/#easing */
		});
	});
	</script>
	<!-- Start Alexa Certify Javascript -->
	<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/BackToTop.jquery.js"></script>
	<script type="text/javascript" src="https://d31qbv1cthcecs.cloudfront.net/atrk.js"></script><script type="text/javascript">_atrk_opts = { atrk_acct: "fJHvg1awAe00iq", domain:"oo.sg"}; atrk ();</script><noscript>&lt;img src="https://d5nxst8fruw4z.cloudfront.net/atrk.gif?account=fJHvg1awAe00iq" style="display:none" height="1" width="1" alt="" /&gt;</noscript>
	<!-- End Alexa Certify Javascript -->

</body></html>