<?php

	include ('constants.php');
	security_check();

	$target = "uploads/";

	$image = addslashes(file_get_contents($_FILES['image']['tmp_name'])); //SQL Injection defence!
	$image_name = addslashes($_FILES['image']['name']);

	$temp = explode(".", $_FILES["image"]["name"]);
	$extension = end($temp);

	$random_bytes = mcrypt_create_iv(12, MCRYPT_DEV_URANDOM);
	$strrandomname = base64_encode($random_bytes).'.'.$extension;
	$strrandomname = str_replace('/', '_', $strrandomname);
	$target = $target . $strrandomname;
	if(move_uploaded_file($_FILES['image']['tmp_name'], $target))
	{
		//Tells you if its all ok
		//echo "The file ". basename( $_FILES['uploadedfile']['name']). " has been uploaded, and your information has been added to the directory";

		$data = file_get_contents($target);
		$arr = explode("\r\n", $data);

		foreach ($arr as $key=>$line)
		{
			if ($key > 0)
			{
				if (trim($line) != "")
				{
					$pieces = explode(',', $line);
					$company_name = trim($pieces[0]);
					$ticker_bloomberg = trim($pieces[1]);
					$ticker_reuters = trim($pieces[2]);
					$ticker_yahoo = trim($pieces[3]);
					$country = trim($pieces[4]);
					$industry = trim($pieces[5]);
					$company_url = trim($pieces[6]);

					$cnt1 = 0;
					if ($ticker_bloomberg != "")
					{
						/*$sql = 'select * from tb_site where ticker_bloomberg = "'.$ticker_bloomberg.'"';
						$res = mysql_query($sql);
						$cnt1 = mysql_num_rows($res);*/

						$sql = 'delete from tb_site where ticker_bloomberg = "'.$ticker_bloomberg.'"';
						mysql_query($sql);
					}

					$cnt2 = 0;
					if ($ticker_reuters != "")
					{
						/*$sql = 'select * from tb_site where ticker_reuters = "'.$ticker_reuters.'"';
						$res = mysql_query($sql);
						$cnt2 = mysql_num_rows($res);*/

						$sql = 'delete from tb_site where ticker_reuters = "'.$ticker_reuters.'"';
						mysql_query($sql);
					}

					$cnt3 = 0;
					if ($ticker_yahoo != "")
					{
						/*$sql = 'select * from tb_site where ticker_yahoo = "'.$ticker_yahoo.'"';
						$res = mysql_query($sql);
						$cnt3 = mysql_num_rows($res);*/

						$sql = 'delete from tb_site where ticker_yahoo = "'.$ticker_yahoo.'"';
						mysql_query($sql);
					}

					$sql = 'delete from tb_site where company_name="'.$company_name.'"';
					$res = mysql_query($sql);

					if ($cnt1+$cnt2+$cnt3 == 0)
					{
						$sql = 'insert into tb_site set ';
						$sql .= 'company_name="'.$company_name.'",';
						$sql .= 'country="'.$country.'",';
						$sql .= 'industry="'.$industry.'",';
						$sql .= 'ticker_bloomberg="'.$ticker_bloomberg.'",';
						$sql .= 'ticker_reuters="'.$ticker_reuters.'",';
						$sql .= 'ticker_yahoo="'.$ticker_yahoo.'",';
						$sql .= 'company_url="'.$company_url.'",';
						$sql .= 'del_flag="0"';
						mysql_query($sql);
					}
				}
			}
		}


	}
?>