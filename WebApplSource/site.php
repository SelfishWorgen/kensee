<?php
	include ('constants.php');
	include ('include/encrypt.php');
	security_check();	

	set_time_limit(0);

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
			$opt = $_POST['opt'];

			$sql = 'select * from tb_site';
			if ($opt == '0')
			{
			}
			else if ($opt == '1')
			{
				$sql .= ' where del_flag="1"';
			}
			else if ($opt == '2')
			{
				$sql .= ' where del_flag="0"';
			}

			$res = mysql_query($sql);
			$ret_data = array();
			while ($arr = mysql_fetch_array($res))
				array_push($ret_data, $arr);
	
			$html = '';
			if (count($ret_data) == 0)
			{
				$html = '<tr class="" id="output4" rel="1455" style="opacity: 200; height:50px; cursor:pointer;" ><td colspan="9" align="center">No Site</td></tr>';
			}
			else
			{
				foreach($ret_data as $key=>$item)
				{
					$action_html1= '';
					if ($item['del_flag'] == '0')
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/wrong.png" style="width:15px;"></a>';
					else
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/tick.png" style="width:15px;"></a>';

					$action_html = '<a href="javascript:void(0);" onclick="edit('.$item['id'].')"><img src="'.$SITEURL.'/img/edit.jpg" style="width:15px;"></a>';

					if ($item['del_flag'] == '0')
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer;" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
					else
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer; color:lightgray" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
				}
			}

			echo $html; exit;
		}
		else if ($postflag == '2')
		{
			$id = $_POST['id'];
			$opt = $_POST['opt'];
			$enable_flag = $_POST['enable_flag'];

			$sql = 'update tb_site set ';
			$sql .= 'del_flag="'.(1 - intval($enable_flag)).'" where id="'.$id.'"';
			mysql_query($sql);

			$sql = 'select * from tb_site where id="'.$id.'"';
			$res = mysql_query($sql);
			$site_data = mysql_fetch_array($res);

			$sql = 'delete from tb_news where company_name="'.$site_data['company_name'].'"';
			mysql_query($sql);

			$sql = 'select * from tb_site';
			if ($opt == '0')
			{
			}
			else if ($opt == '1')
			{
				$sql .= ' where del_flag="1"';
			}
			else if ($opt == '2')
			{
				$sql .= ' where del_flag="0"';
			}

			$res = mysql_query($sql);
			$ret_data = array();
			while ($arr = mysql_fetch_array($res))
				array_push($ret_data, $arr);
	
			$html = '';
			if (count($ret_data) == 0)
			{
				$html = '<tr class="" id="output4" rel="1455" style="opacity: 200; height:50px; cursor:pointer;" ><td colspan="9" align="center">No Site</td></tr>';
			}
			else
			{
				foreach($ret_data as $key=>$item)
				{
					$action_html1= '';
					if ($item['del_flag'] == '0')
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/wrong.png" style="width:15px;"></a>';
					else
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/tick.png" style="width:15px;"></a>';

					$action_html = '<a href="javascript:void(0);" onclick="edit('.$item['id'].')"><img src="'.$SITEURL.'/img/edit.jpg" style="width:15px;"></a>';

					if ($item['del_flag'] == '0')
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer;" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
					else
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer; color:lightgray" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
				}
			}

			echo $html; exit;
		}
		else if ($postflag == '3')
		{
			$opt = $_POST['opt'];

			$sql = 'update tb_site set ';
			$sql .= 'del_flag="1"';
			mysql_query($sql);

			$sql = 'select * from tb_site where del_flag="1"';
			$res = mysql_query($sql);
			while ($site_data = mysql_fetch_array($res))
			{
				$sql = 'delete from tb_news where company_name="'.$site_data['company_name'].'"';
				mysql_query($sql);
			}

			$sql = 'select * from tb_site';
			if ($opt == '0')
			{
			}
			else if ($opt == '1')
			{
				$sql .= ' where del_flag="1"';
			}
			else if ($opt == '2')
			{
				$sql .= ' where del_flag="0"';
			}

			$res = mysql_query($sql);
			$ret_data = array();
			while ($arr = mysql_fetch_array($res))
				array_push($ret_data, $arr);
	
			$html = '';
			if (count($ret_data) == 0)
			{
				$html = '<tr class="" id="output4" rel="1455" style="opacity: 200; height:50px; cursor:pointer;" ><td colspan="9" align="center">No Site</td></tr>';
			}
			else
			{
				foreach($ret_data as $key=>$item)
				{
					$action_html1= '';
					if ($item['del_flag'] == '0')
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/wrong.png" style="width:15px;"></a>';
					else
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/tick.png" style="width:15px;"></a>';

					$action_html = '<a href="javascript:void(0);" onclick="edit('.$item['id'].')"><img src="'.$SITEURL.'/img/edit.jpg" style="width:15px;"></a>';

					if ($item['del_flag'] == '0')
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer;" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
					else
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer; color:lightgray" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
				}
			}

			echo $html; exit;
		}
		else if ($postflag == '4')
		{
			$opt = $_POST['opt'];

			$sql = 'update tb_site set ';
			$sql .= 'del_flag="0"';
			mysql_query($sql);

			$sql = 'select * from tb_site';
			if ($opt == '0')
			{
			}
			else if ($opt == '1')
			{
				$sql .= ' where del_flag="1"';
			}
			else if ($opt == '2')
			{
				$sql .= ' where del_flag="0"';
			}

			$res = mysql_query($sql);
			$ret_data = array();
			while ($arr = mysql_fetch_array($res))
				array_push($ret_data, $arr);
	
			$html = '';
			if (count($ret_data) == 0)
			{
				$html = '<tr class="" id="output4" rel="1455" style="opacity: 200; height:50px; cursor:pointer;" ><td colspan="9" align="center">No Site</td></tr>';
			}
			else
			{
				foreach($ret_data as $key=>$item)
				{
					$action_html1= '';
					if ($item['del_flag'] == '0')
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/wrong.png" style="width:15px;"></a>';
					else
						$action_html1 = '<a href="javascript:void(0);" onclick="enable('.$item['id'].','.$item['del_flag'].')"><img src="'.$SITEURL.'/img/tick.png" style="width:15px;"></a>';

					$action_html = '<a href="javascript:void(0);" onclick="edit('.$item['id'].')"><img src="'.$SITEURL.'/img/edit.jpg" style="width:15px;"></a>';

					if ($item['del_flag'] == '0')
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer;" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
					else
						$html .= '<tr class="" id="output4" rel="1455" style="opacity: 200; height:30px; cursor:pointer; color:lightgray" ><td valign="top">'.$item['industry'].'</td><td valign="top">'.$item['company_name'].'</td><td valign="top">'.$item['country'].'</td><td valign="top">'.$item['ticker_bloomberg'].'</td><td valign="top">'.$item['ticker_reuters'].'</td><td valign="top">'.$item['ticker_yahoo'].'</td><td valign="top">'.$item['company_url'].'</td><td valign="top">'.$action_html.'</td><td valign="top">'.$action_html1.'</td></tr>';
				}
			}

			echo $html; exit;
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

<link rel="shortcut icon" type="image/jpg" href="<?php echo $SITEURL; ?>/images/logo_small.jpg">

<link rel="icon" type="image/jpg" href="<?php echo $SITEURL; ?>/images/logo_big.jpg">

<script type="text/javascript" async="" src="http://www.google-analytics.com/plugins/ga/inpage_linkid.js" id="undefined"></script><script type="text/javascript" async="" src="http://stats.g.doubleclick.net/dc.js"></script><script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery-ui.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery.validate.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/bootstrap.js"></script>

<script type="text/javascript" src="<?php echo $SITEURL; ?>/js/jquery.jcarousel.min.js"></script>

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
		background-color: rgb(45,184,125);

	}
	.ph-btn-green:hover, .ph-btn-green:focus, .ph-btn-green:active {
		background-color: #3AC162;
		border-color: #3AC162;
	}
	.ph-button {
		border-style: solid;
		border-width: 0px 0px 3px;
		box-shadow: 0 -1px 0 rgba(255, 255, 255, 0.1) inset;
		color: #FFFFFF;	   
		border-radius: 16px;
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
		top:208px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-bottom: 10px solid #fff;
		display:inline;
		position:absolute;
	}

	.arrow-up-color {
		top:208px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-bottom: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}

	.arrow-up:hover {
		top:208px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-bottom: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}

	.arrow-down {
		top:208px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-top: 10px solid #fff;
		display:inline;
		position:absolute;
	}
	.arrow-down:hover {
		top:208px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-top: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}

	.arrow-down-color {
		top:208px;
		width: 0; 
		height: 0; 
		border-left: 7px solid transparent;
		border-right: 7px solid transparent;
		border-top: 10px solid #00A2E8;
		display:inline;
		position:absolute;
	}
	</style>

	<link rel="stylesheet" href="<?php echo $SITEURL; ?>/css/search_form.css" type="text/css">

<script>

$(function(){
	$('#theFile').change(function(e){
		uploadFile();
	});

	load_SiteData('0');
});

function load_SiteData(opt)
{
	$('#tbody_content').html('<tr class="output4 theStylingClass" style="cursor:pointer;"><td colspan="9" align="center"><img src="img/loading.gif"></td></tr>');
	$.ajax({
		type: "POST", 
		url: "site.php",
		data : {postflag:'1', opt:opt},
		success : function(data){
			$('#tbody_content').html(data);
		}
	});
}

function enable(id, enable_flag)
{
	if (enable_flag == '0')
	{
		if (confirm('Will you disable this site?')) {
		}
		else
		{
			return;
		}
	}
	var opt = $('[name=opt_show]').val();
	$.ajax({
		type: "POST", 
		url: "site.php",
		data : {postflag:'2', id:id, enable_flag:enable_flag, opt:opt},
		success : function(data){
			$('#tbody_content').html(data);
		}
	});
}

function add()
{
	window.location.href="<?php echo $SITEURL; ?>/site_edit.php?id=-1";
}

function edit(id)
{
	window.location.href="<?php echo $SITEURL; ?>/site_edit.php?id=" + id;
}

function import_csv()
{
	var node = document.getElementById('theFile');
	var evt = document.createEvent("MouseEvents");
	evt.initEvent("click", true, false);
	node.dispatchEvent(evt);
}

function uploadFile()
{
	if ($('#theFile').val() == "")
	{
		alert("Please choose another file!");
		return;
	}
	var files = $('#theFile')[0].files;
	
	// Serialize the form data
	var data = new FormData();
	
	// You should sterilise the file names
	$.each(files, function(key, value)
	{
		data.append('image', value);
	});

	//$('#msg').text("Uploading now...");
	var type = 0;

	$('#tbody_content').html('<tr class="output4 theStylingClass" style="cursor:pointer;"><td colspan="9" align="center"><img src="img/loading.gif"></td></tr>');

	$.ajax({
		url: 'upload.php',
		data: data,
		cache: false,
		contentType: false,
		processData: false,
		type: 'POST',
		success: function(data){
			console.log(data);
			var opt = $('[name=opt_show]').val();
			load_SiteData(opt);
			//alert("Successfully Uploaded!");
		}
	});
}

function show_option()
{
	var opt = $('[name=opt_show]').val();
	load_SiteData(opt);
}

function disable_all()
{
	if (confirm('Will you disable all sites?')) {
	}
	else
	{
		return;
	}

	var opt = $('[name=opt_show]').val();
	$.ajax({
		type: "POST", 
		url: "site.php",
		data : {postflag:'3', opt:opt},
		success : function(data){
			$('#tbody_content').html(data);
		}
	});
}

function enable_all()
{
	var opt = $('[name=opt_show]').val();
	$.ajax({
		type: "POST", 
		url: "site.php",
		data : {postflag:'4', opt:opt},
		success : function(data){
			console.log(data);
			$('#tbody_content').html(data);
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

				<div class="middle_column" style="width:1100px; min-width:850px;">
					<!--<div class="shadow_top"></div>-->
					<div id="home_page" class="middle_container" style="width:100%">

						<div class="middle" style="width:100%; min-width:850px;">
							<p>&nbsp;</p>
						
							<div class="clear"></div>

							<div class="carousel_content">
								<div class="carousel" style="width:100%; margin-left:-10px;">
									
									<div class="clear"></div>
									<div style="margin-top:30px; width:100%">
										<table width="100%" cellpadding="10" id="information" cellspacing="0">
											<tr>
												<td colspan='2'>
													<select name="opt_show" style="-webkit-border-radius:0px; margin-top:-15px;" onchange="show_option();">
														<option value="0">Show All</option>
														<option value="1">Show Disabled</option>
														<option value="2">Show Enabled</option>
													</select>
												</td>
												<td colspan='7' style="text-align:right">
													<input type="file" id="theFile" name="image" style="display:none"/>
													<a href="javascript:void(0);" class="ph-button ph-btn-green" onclick="disable_all();"><span style="color:#ffffff; font-size:14px;">Disable All</span></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
													<a href="javascript:void(0);" class="ph-button ph-btn-green" onclick="enable_all();"><span style="color:#ffffff; font-size:14px;">Enable All</span></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
													<a href="javascript:void(0);" class="ph-button ph-btn-green" onclick="import_csv();"><span style="color:#ffffff; font-size:14px;">Import</span></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
													<a href="javascript:void(0);" class="ph-button ph-btn-green" onclick="add();"><span style="color:#ffffff; font-size:14px;">Add Site</span></a>
												</td>
											</tr>
										</table>
										<table width="100%" cellpadding="10" id="information" cellspacing="0" style="border:2px solid lightgray; font-size:13px;">
											<thead>
												<tr style="font-size:16px; color:#636363;background:#C6C6C6;width:80%;">
													<th class="bsorttable_nosort" style="width:15%; cursor:pointer;">Industry</th>
													<th class="bsorttable_nosort" style="width:26%; cursor:pointer;">Company Name</th>
													<th class="bsorttable_nosort" style="width:8%; cursor:pointer;">Country</th>
													<th class="bsorttable_nosort" style="width:8%; cursor:pointer;">Bloomberg</th>
													<th class="bsorttable_nosort" style="width:8%; cursor:pointer;">Reuters</th>
													<th class="bsorttable_nosort" style="width:8%; cursor:pointer;">Yahoo</th>
													<th class="bsorttable_nosort" style="width:9%; cursor:pointer;">Url</th>
													<th class="bsorttable_nosort" style="width:9%; cursor:pointer;">&nbsp;&nbsp;</th>
													<th class="bsorttable_nosort" style="width:9%; cursor:pointer;">&nbsp;&nbsp;</th>
												</tr>
											</thead>
											<tbody id="tbody_content">
											</tbody>
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

			</div>
			
			<div class="clear"></div>
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