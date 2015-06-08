<?php

	if(!isset($_SESSION)) 
    { 
        session_start(); 
    }
	
	/*$dbName = 'hus1018202125656';
	$user = 'hus1018202125656';
	$pass = 'Tempest25!@';
	$server = 'hus1018202125656.db.6371085.hostedresource.com';*/

	$dbName = 'db_news';
	$user = 'root';
	$pass = '';
	$server = 'localhost';

	$pgSize = 20;

	/*Database Connection*/
	$conn = mysql_connect($server,$user,$pass);
	if (!$conn) {
		die('Could not connect: ' . mysql_error());
	}

	$db_selected = mysql_select_db($dbName, $conn);

	//$SITEURL = 'http://158.85.229.36:8888/kensee';
	$SITEURL = 'http://localhost:8888/kensee';

	////////////////FUNCTION AREA
	function security_check(){
		if(trim($_SESSION['USER'])==''){
			$_SESSION['USER'] = '';
			$_SESSION['USER_NAME'] = '';
			$_SESSION['USER_EMAIL'] = '';
			$_SESSION['USER_ID'] ='';
			$_SESSION['ADMIN'] = '';
			header('Location: login.php');
			exit;
		}
	}

	function security_check_admin(){
		if(trim($_SESSION['ADMIN_USER'])==''){
			$_SESSION['ADMIN_USER'] = '';
			$_SESSION['ADMIN_USER_NAME'] = '';
			$_SESSION['ADMIN_USER_EMAIL'] = '';
			$_SESSION['ADMIN_USER_ID'] ='';
			header('Location: login.php');
			exit;
		}
	}

	function validuser($username,$password)
	{
		$sql = "select * from tb_user where (username='".$username."' OR email='".$username."') AND passkey='".$password."'";
		$temp = mysql_fetch_array(mysql_query($sql));
		if($temp)
		{
			/*Set SESSIONS*/
			$_SESSION['USER'] = $temp['username'];
			$_SESSION['USER_NAME'] = trim($temp['firstName'].' '.$temp['lastName']);
			$_SESSION['USER_EMAIL'] = $temp['email'];
			$_SESSION['USER_ID'] = $temp['id'];
			$_SESSION['ADMIN'] = '0';
			if ($temp['username'] == 'admin')
				$_SESSION['ADMIN'] = '1';

			return true;
		}
		else
		{
			return false;
		}
	}//eofunc

	function validuser_admin($username,$password)
	{
		$sql = "select * from tb_admin where (username='".$username."' OR email='".$username."') AND passkey='".$password."'";
		$temp = mysql_fetch_array(mysql_query($sql));
		if($temp)
		{
			/*Set SESSIONS*/
			$_SESSION['ADMIN_USER'] = $temp['username'];
			$_SESSION['ADMIN_USER_NAME'] = trim($temp['firstName'].' '.$temp['lastName']);
			$_SESSION['ADMIN_USER_EMAIL'] = $temp['email'];
			$_SESSION['ADMIN_USER_ID'] = $temp['id'];

			return true;
		}
		else
		{
			return false;
		}
	}//eofunc

	function kill_session(){
		$_SESSION['USER'] = '';
		$_SESSION['USER_NAME'] = '';
		$_SESSION['USER_EMAIL'] = '';
		$_SESSION['USER_ID'] = '';
		$_SESSION['ADMIN'] = '';
		header('Location: login.php');
	}

	function kill_session_admin(){
		$_SESSION['ADMIN_USER'] = '';
		$_SESSION['ADMIN_USER_NAME'] = '';
		$_SESSION['ADMIN_USER_EMAIL'] = '';
		$_SESSION['ADMIN_USER_ID'] = '';
		header('Location: login.php');
	}
?>