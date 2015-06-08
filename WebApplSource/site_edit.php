<?php
	include ('constants.php');
	security_check();	

	if (isset($_POST['postFlag']))
	{
		if ($_POST['postFlag'] == '1')
		{
			$id = $_POST['id'];
			if ($id == '-1') // INSERT
			{
				$sql = 'insert into tb_site set ';
				$sql .= 'company_name="'.$_POST['company_name'].'",';
				$sql .= 'country="'.$_POST['country'].'",';
				$sql .= 'industry="'.$_POST['industry'].'",';
				$sql .= 'ticker_bloomberg="'.$_POST['ticker_bloomberg'].'",';
				$sql .= 'ticker_reuters="'.$_POST['ticker_reuters'].'",';
				$sql .= 'ticker_yahoo="'.$_POST['ticker_yahoo'].'",';
				$sql .= 'company_url="'.$_POST['company_url'].'",';
				$sql .= 'del_flag="0"';
				mysql_query($sql);
			}
			else // UPDATE
			{
				$sql = 'update tb_site set ';
				$sql .= 'company_name="'.$_POST['company_name'].'",';
				$sql .= 'country="'.$_POST['country'].'",';
				$sql .= 'industry="'.$_POST['industry'].'",';
				$sql .= 'ticker_bloomberg="'.$_POST['ticker_bloomberg'].'",';
				$sql .= 'ticker_reuters="'.$_POST['ticker_reuters'].'",';
				$sql .= 'ticker_yahoo="'.$_POST['ticker_yahoo'].'",';
				$sql .= 'company_url="'.$_POST['company_url'].'"';
				$sql .= ' where id="'.$id.'"';
				mysql_query($sql);
			}
			header ('Location: site.php');
			exit;
		}
		else if ($_POST['postFlag'] == '2')
		{
			$ticker_bloomberg = $_POST['ticker_bloomberg'];
			$ticker_yahoo = $_POST['ticker_yahoo'];
			$ticker_reuters = $_POST['ticker_reuters'];
			$id = $_POST['id'];

			$cnt1 = 0;
			if ($ticker_bloomberg != "")
			{
				$sql = 'select * from tb_site where ticker_bloomberg = "'.$ticker_bloomberg.'" and id<>"'.$id.'"';
				$res = mysql_query($sql);
				$cnt1 = mysql_num_rows($res);
			}
			else
				$cnt1 = -1;

			$cnt2 = 0;
			if ($ticker_reuters != "")
			{
				$sql = 'select * from tb_site where ticker_reuters = "'.$ticker_reuters.'" and id<>"'.$id.'"';
				$res = mysql_query($sql);
				$cnt2 = mysql_num_rows($res);
			}
			else
				$cnt2 = -1;

			$cnt3 = 0;
			if ($ticker_yahoo != "")
			{
				$sql = 'select * from tb_site where ticker_yahoo = "'.$ticker_yahoo.'" and id<>"'.$id.'"';
				$res = mysql_query($sql);
				$cnt3 = mysql_num_rows($res);
			}
			else
				$cnt3 = -1;

			$ret = $cnt1.' ||| '.$cnt2.' ||| '.$cnt3;
			echo $ret; exit;
		}
	}

	$id = $_GET['id'];
	$sql = 'select * from tb_site where id="'.$id.'"';
	$res = mysql_query($sql);
	$data = mysql_fetch_array($res);

	$ssss = array(
		'AD' => 'Andorra',
		'AE' => 'United Arab Emirates',
		'AF' => 'Afghanistan',
		'AG' => 'Antigua and Barbuda',
		'AI' => 'Anguilla',
		'AL' => 'Albania',
		'AM' => 'Armenia',
		'AN' => 'Netherlands Antilles',
		'AO' => 'Angola',
		'AQ' => 'Antarctica',
		'AR' => 'Argentina',
		'AS' => 'American Samoa',
		'AT' => 'Austria',
		'AU' => 'Australia',
		'AW' => 'Aruba',
		'AZ' => 'Azerbaijan',
		'BA' => 'Bosnia-Herzegovina',
		'BB' => 'Barbados',
		'BD' => 'Bangladesh',
		'BE' => 'Belgium',
		'BF' => 'Burkina Faso',
		'BG' => 'Bulgaria',
		'BH' => 'Bahrain',
		'BI' => 'Burundi',
		'BJ' => 'Benin',
		'BM' => 'Bermuda',
		'BN' => 'Brunei Darrussalam',
		'BO' => 'Bolivia',
		'BR' => 'Brazil',
		'BS' => 'Bahamas',
		'BT' => 'Bhutan',
		'BV' => 'Bouvet Island',
		'BW' => 'Botswana',
		'BY' => 'Belarus',
		'BZ' => 'Belize',
		'CA' => 'Canada',
		'CC' => 'Cocos (Keeling) Islands',
		'CD' => 'Congo (Democratic Republic)',
		'CF' => 'Central African Republic',
		'CG' => 'Congo, Republic of',
		'CH' => 'Switzerland',
		'CI' => 'Ivory Coast (Cote D`Ivoire)',
		'CK' => 'Cook Islands',
		'CL' => 'Chile',
		'CM' => 'Cameroon',
		'CN' => 'China',
		'CO' => 'Colombia',
		'CR' => 'Costa Rica',
		'CU' => 'Cuba',
		'CV' => 'Cape Verde',
		'CS' => 'Christmas Island',
		'CY' => 'Cyprus',
		'CZ' => 'Czech Republic',
		'DE' => 'Germany',
		'DJ' => 'Djibouti',
		'DK' => 'Denmark',
		'DM' => 'Dominica',
		'DO' => 'Dominican Republic',
		'DZ' => 'Algeria',
		'EC' => 'Ecuador',
		'EE' => 'Estonia',
		'EG' => 'Egypt',
		'EH' => 'Western Sahara',
		'ER' => 'Eritrea',
		'ES' => 'Spain',
		'ET' => 'Ethiopia',
		'FI' => 'Finland',
		'FJ' => 'Fiji',
		'FK' => 'Falkland Islands',
		'FM' => 'Micronesia',
		'FO' => 'Faroe Islands',
		'FR' => 'France',
		'GA' => 'Gabon',
		'GB' => 'United Kingdom',
		'GD' => 'Grenada',
		'GE' => 'Georgia',
		'GF' => 'French Guiana',
		'GH' => 'Ghana',
		'GI' => 'Gibraltar',
		'GL' => 'Greenland',
		'GM' => 'Gambia',
		'GN' => 'Guinea',
		'GP' => 'Guadeloupe',
		'GQ' => 'Equatorial Guinea',
		'GR' => 'Greece',
		'GS' => 'South Georgia',
		'GT' => 'Guatemala',
		'GU' => 'Guam',
		'GW' => 'Guinea-Bissau',
		'GY' => 'Guyana',
		'HK' => 'Hong Kong',
		'HM' => 'Heard and McDonald Islands',
		'HN' => 'Honduras',
		'HR' => 'Croatia',
		'HT' => 'Haiti',
		'HU' => 'Hungary',
		'ID' => 'Indonesia',
		'IE' => 'Ireland',
		'IL' => 'Israel',
		'IN' => 'India',
		'IO' => 'British Indian Ocean Territory',
		'IQ' => 'Iraq',
		'IR' => 'Iran',
		'IS' => 'Iceland',
		'IT' => 'Italy',
		'JM' => 'Jamaica',
		'JO' => 'Jordan',
		'JP' => 'Japan',
		'KE' => 'Kenya',
		'KG' => 'Kyrgyzstan',
		'KH' => 'Cambodia',
		'KI' => 'Kiribati',
		'KM' => 'Comoros',
		'KN' => 'Saint Kitts & Nevis Anguilla',
		'KP' => 'North Korea',
		'KR' => 'South Korea',
		'KW' => 'Kuwait',
		'KY' => 'Cayman Islands',
		'KZ' => 'Kazakhstan',
		'LA' => 'Laos',
		'LB' => 'Lebanon',
		'LC' => 'Saint Lucia',
		'LI' => 'Liechtenstein',
		'LK' => 'Sri Lanka',
		'LR' => 'Liberia',
		'LS' => 'Lesotho',
		'LT' => 'Lithuania',
		'LU' => 'Luxembourg',
		'LV' => 'Latvia',
		'LY' => 'Libya',
		'MA' => 'Morocco',
		'MC' => 'Monaco',
		'MD' => 'Moldova',
		'MG' => 'Madagascar',
		'MH' => 'Marshall Islands',
		'MK' => 'Macedonia',
		'ML' => 'Mali',
		'MM' => 'Myanmar',
		'MN' => 'Mongolia',
		'MO' => 'Macau',
		'MP' => 'Northern Mariana Islands',
		'MQ' => 'Martinique',
		'MR' => 'Mauritania',
		'MS' => 'Montserrat',
		'MT' => 'Malta',
		'MU' => 'Mauritius',
		'Mv' => 'Maldives',
		'MW' => 'malawi',
		'MX' => 'Mexico',
		'MY' => 'Malaysia',
		'MZ' => 'Mozambique',
		'NA' => 'Namibia',
		'NC' => 'New Caledonia',
		'NE' => 'Niger',
		'NF' => 'Norfolk Island',
		'NG' => 'Nigeria',
		'NI' => 'Nicaragua',
		'NL' => 'Netherlands',
		'NO' => 'Norway',
		'NP' => 'Nepal',
		'NR' => 'Nauru',
		'NU' => 'Niue',
		'NZ' => 'New Zealand',
		'OM' => 'Oman',
		'PA' => 'Panama',
		'PE' => 'Peru',
		'PF' => 'Polynesia',
		'PG' => 'Papua New Guinea',
		'PH' => 'Phillipines',
		'PK' => 'Pakistan',
		'PL' => 'Poland',
		'PM' => 'Saint Pierre and Miquelon',
		'PN' => 'Pitcairn Island',
		'PR' => 'Puerto Rico',
		'PS' => 'Palestinian Territories',
		'PT' => 'Portugal',
		'PW' => 'Palau',
		'PY' => 'Paraguay',
		'QA' => 'Qatar',
		'RE' => 'Reunion',
		'RO' => 'Romania',
		'RU' => 'Russian Federation',
		'RW' => 'Rwanda',
		'SA' => 'Saudi Arabia',
		'SB' => 'Solomon Islands',
		'SC' => 'Seychelles',
		'SD' => 'Sudan',
		'SE' => 'Sweden',
		'SG' => 'Singapore',
		'SH' => 'Saint Helena',
		'SI' => 'Slovenia',
		'SJ' => 'Svalbard and Jan Mayen Islands',
		'SK' => 'Slovak Republic',
		'SL' => 'Sierra Leone',
		'SM' => 'San Marino',
		'SN' => 'Senegal',
		'SO' => 'Somalia',
		'SR' => 'Suriname',
		'ST' => 'Sao Tome and Principe',
		'SV' => 'El Salvador',
		'SY' => 'Syria',
		'SZ' => 'Swaziland',
		'TC' => 'Turks and Caicos Islands',
		'TD' => 'Chad',
		'TF' => 'French Southern Territories',
		'TG' => 'Togo',
		'TH' => 'Thailand',
		'TJ' => 'Tajikistan',
		'TK' => 'Tokelau',
		'TM' => 'Turkmenistan',
		'TN' => 'Tunisia',
		'TO' => 'Tonga',
		'TP' => 'East Timor',
		'TR' => 'Turkey',
		'TT' => 'Trinidad and Tobago',
		'TV' => 'Tuvalu',
		'TW' => 'Taiwan',
		'TZ' => 'Tanzania',
		'UA' => 'Ukraine',
		'UG' => 'Uganda',
		'UM' => 'USA Minor Outlying Islands',
		'US' => 'United States',
		'UY' => 'Uruguay',
		'UZ' => 'Uzbekistan',
		'VA' => 'Holy See (Vatican City State)',
		'VC' => 'Saint Vincent & Grenadines',
		'VE' => 'Venezuela',
		'VG' => 'Virgin Islands (British)',
		'VI' => 'Virgin Islands (USA)',
		'VN' => 'Vietnam',
		'VU' => 'Vanuatu',
		'WF' => 'Wallis and Futuna Islands',
		'WS' => 'Samoa',
		'YE' => 'Yemen',
		'YT' => 'Mayotte',
		'YU' => 'Yugoslavia',
		'ZA' => 'South Africa',
		'ZM' => 'Zambia',
		'ZW' => 'Zimbabwe'
	);

	usort($ssss, function($a, $b) {
		return strcmp($a, $b);
	});
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
		background-color: #5FCF80;

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

function submit_form()
{
	var ticker_bloomberg = $('#ticker_bloomberg').val();
	var ticker_reuters = $('#ticker_reuters').val();
	var ticker_yahoo = $('#ticker_yahoo').val();
	var id = $('#id').val();
	$.ajax({
		type: "POST", 
		url: "site_edit.php",
		data : {postFlag:'2', id:id, ticker_bloomberg:ticker_bloomberg, ticker_reuters:ticker_reuters, ticker_yahoo:ticker_yahoo},
		success : function(data){
			console.log(data);
			var pieces = data.split(' ||| ');
			if (parseInt(pieces[0]) > 0)
			{
				alert('Bloomberg Ticker already exists.');
				$('#ticker_bloomberg').focus();
				return;
			}

			if (parseInt(pieces[1]) > 0)
			{
				alert('Reuters Ticker already exists.');
				$('#ticker_reuters').focus();
				return;
			}

			if (parseInt(pieces[2]) > 0)
			{
				alert('Yahoo Ticker already exists.');
				$('#ticker_yahoo').focus();
				return;
			}
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

				<div class="middle_column" style="width:1100px; min-width:850px;">
					<!--<div class="shadow_top"></div>-->
					<div id="home_page" class="middle_container" style="width:100%">

						<div class="middle" style="width:100%; min-width:850px;">
							<p>&nbsp;</p>
						
							<div class="clear"></div>

							<div class="carousel_content">
								<div class="carousel" style="width:100%; margin-left:-10px;">
									
									<form name="mainForm" method="post" action="site_edit.php">
									<input type="hidden" name="postFlag" value="1">
									<input type="hidden" name="id" id="id" value="<?php echo $id; ?>">

									<div class="clear"></div>
									<div style="margin-top:30px; width:100%">
										<div class="carousel">
											<?php if ($id == '-1') { ?>
											<h3 itemprop="name" style="font-weight:normal;">Add a site</h1>
											<?php } else { ?>
											<h3 itemprop="name" style="font-weight:normal;">Edit a site</h1>
											<?php } ?>
										</div>

										<div class="carousel">
											<h4 style="font-weight:normal;">Company Name</h4>
											<div class="carousel" style="margin-left:15px;margin-top:15px;vertical-align:top;">
												<input type="text" name="company_name" id="company_name" value="<?php echo $data['company_name']; ?>" style="-webkit-border-radius:0px; height:30px; width:350px; margin-top:-15px;" autocomplete="off">
											</div>
										</div>

										<div class="carousel">
											<h4 style="font-weight:normal;">Choose Industry</h4>
											<div class="carousel" style="margin-left:15px;margin-top:15px;vertical-align:top;">
												<select name="industry" style="-webkit-border-radius:0px; margin-top:-15px;">
													<option value="Real Estate" <?php if ($data['industry'] == 'Real Estate') echo 'selected'; ?>>Real Estate</option>
													<option value="Chemicals" <?php if ($data['industry'] == 'Chemicals') echo 'selected'; ?>>Chemicals</option>
												</select>
											</div>
										</div>

										<div class="carousel">
											<h4 style="font-weight:normal;">Country</h4>
											<div class="carousel" style="margin-left:15px;margin-top:15px;vertical-align:top;">
												<select name="country" style="-webkit-border-radius:0px; margin-top:-15px;">
													<?php foreach($ssss as $key=>$val){ ?>
													<option value="<?php echo $val; ?>" <?php if (($pos=strpos($data['country'], $val))!==false) echo 'selected'; ?>><?php echo $val; ?></option>
													<?php } ?>
												</select>
											</div>
										</div>

										<div class="carousel">
											<h4 style="font-weight:normal;">Bloomberg Ticker</h4>
											<div class="carousel" style="margin-left:15px;margin-top:15px;vertical-align:top;">
												<input type="text" name="ticker_bloomberg" id="ticker_bloomberg" value="<?php echo $data['ticker_bloomberg']; ?>" style="-webkit-border-radius:0px; height:30px; width:350px; margin-top:-15px;" autocomplete="off">
											</div>
										</div>

										<div class="carousel">
											<h4 style="font-weight:normal;">Reuters Ticker</h4>
											<div class="carousel" style="margin-left:15px;margin-top:15px;vertical-align:top;">
												<input type="text" name="ticker_reuters" id="ticker_reuters" value="<?php echo $data['ticker_reuters']; ?>" style="-webkit-border-radius:0px; height:30px; width:350px; margin-top:-15px;" autocomplete="off">
											</div>
										</div>

										<div class="carousel">
											<h4 style="font-weight:normal;">Yahoo Ticker</h4>
											<div class="carousel" style="margin-left:15px;margin-top:15px;vertical-align:top;">
												<input type="text" name="ticker_yahoo" id="ticker_yahoo" value="<?php echo $data['ticker_yahoo']; ?>" style="-webkit-border-radius:0px; height:30px; width:350px; margin-top:-15px;" autocomplete="off">
											</div>
										</div>

										<div class="carousel">
											<h4 style="font-weight:normal;">Company Url</h4>
											<div class="carousel" style="margin-left:15px;margin-top:15px;vertical-align:top;">
												<input type="text" name="company_url" id="company_url" value="<?php echo $data['company_url']; ?>" style="-webkit-border-radius:0px; height:30px; width:350px; margin-top:-15px;" autocomplete="off">
											</div>
										</div>

										<div class="carousel" style="text-align:center;">
											<a href="javascript:void(0);" class="ph-button ph-btn-green" onclick="submit_form();" style="margin-left:35px"><span style="color:#ffffff; font-size:14px;"><?php if ($id!='-1') echo 'Save'; else echo 'Save'; ?></span></a>
										</div>
									</div>
									</form>
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