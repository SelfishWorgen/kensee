<?php
	
	include('constants.php');

	$username = "";
	$pass = "";
	$err = "";
	$confirm = "";

	$un = "";
	$em = "";
	$fn = "";
	$sn = "";

	if (isset($_POST['login'])) // LOGIN
	{
		$username = $_POST['username'];
		$pass = $_POST['password'];

		$ret = validuser($username, md5($pass));
		if ($ret == true)
			header('Location: index.php');
		else
			$err = "Username and Password doesn't match.";
	}
	else if (isset($_POST['signup'])) //SIGNUP
	{
		$un = $_POST['un'];
		$em = $_POST['em'];
		$ps = $_POST['ps'];
		$sn = $_POST['sn'];
		$fn = $_POST['fn'];

		$sql = 'select * from tb_user where username="'.$un.'"';
		$res = mysql_query($sql);
		$cnt = mysql_fetch_array($res);
		if ($cnt > 0)
		{
			echo '1';exit;
		}

		$sql = 'select * from tb_user where email="'.$em.'"';
		$res = mysql_query($sql);
		$cnt = mysql_fetch_array($res);
		if ($cnt > 0)
		{
			echo '2'; exit;
		}

		$sql = 'insert into tb_user set username="'.$un.'",email="'.$em.'",passkey="'.md5($ps).'", firstName="'.$fn.'", lastName="'.$sn.'"';
		mysql_query($sql);
		echo '3';exit;
	}
?>

<!DOCTYPE html>
<!--[if lt IE 7 ]> <html lang="en" class="no-js ie6 lt8"> <![endif]-->
<!--[if IE 7 ]>    <html lang="en" class="no-js ie7 lt8"> <![endif]-->
<!--[if IE 8 ]>    <html lang="en" class="no-js ie8 lt8"> <![endif]-->
<!--[if IE 9 ]>    <html lang="en" class="no-js ie9"> <![endif]-->
<!--[if (gt IE 9)|!(IE)]><!--> <html lang="en" class="no-js"> <!--<![endif]-->
    <head>
        <meta charset="UTF-8" />
        <!-- <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">  -->
        <title>Login </title>
        <meta name="viewport" content="width=device-width, initial-scale=1.0"> 
        <meta name="description" content="Login and Registration Form with HTML5 and CSS3" />
        <meta name="keywords" content="html5, css3, form, switch, animation, :target, pseudo-class" />
        <meta name="author" content="Codrops" />
        <link rel="shortcut icon" type="image/jpg" href="<?php echo $SITEURL; ?>/images/logo_small.jpg">
		<link rel="icon" type="image/jpg" href="<?php echo $SITEURL; ?>/images/logo_big.jpg">
        <link rel="stylesheet" type="text/css" href="css/demo.css" />
        <link rel="stylesheet" type="text/css" href="css/style3.css" />
		<link rel="stylesheet" type="text/css" href="css/animate-custom.css" />
		<script type="text/javascript" src="js/jquery.js"></script>

		<script language="javascript">
		function checkFields()
		{
			var pas = document.getElementById('passwordsignup').value;
			var pas1 = document.getElementById('passwordsignup_confirm').value;

			if (pas != pas1)
			{
				alert("Password doesn't match.");
				return false;
			}

			var un = $('#usernamesignup').val();
			var em = $('#emailsignup').val();
			var fn = $('#firstnamesignup').val();
			var sn = $('#surnamesignup').val();

			if (un == "")
			{
				alert("Enter username!");
				return;
			}

			if (em == "")
			{
				alert("Enter email address!");
				return;
			}

			$.ajax({
				type: "POST", 
				url: "login.php",
				data: {signup:'1', un:un, em:em, ps:pas, fn:fn, sn:sn},
				success: bbbb
			});
		}

		bbbb = function(data){
			if (data == "1")
			{
				$('#msg').html('<h2 style="color:red">Username already exists! Please try another.</h2><BR>');
			}
			else if (data == "2")
			{
				$('#msg').html('<h2 style="color:red">Email already exists! Please try another.</h2><BR>');
			}
			else if (data == "3")
			{
				$('#msg').html('<h2 style="color:green">User was successfully created!</h2><BR>');
				$('#usernamesignup').val('');
				$('#emailsignup').val('');
				$('#passwordsignup').val('');
				$('#passwordsignup_confirm').val('');
				$('#firstnamesignup').val('');
				$('#surnamesignup').val('');
			}
		}
		</script>

		<style type="text/css">
		</style>

    </head>
    <body>
        <div class="container">
            <header>
			<BR><BR>
            </header>
            <section>				
                <div id="container_demo" >
                    <!-- hidden anchor to stop jump http://www.css3create.com/Astuce-Empecher-le-scroll-avec-l-utilisation-de-target#wrap4  -->
                    <a class="hiddenanchor" id="toregister"></a>
                    <a class="hiddenanchor" id="tologin"></a>
                    <div id="wrapper">
                        <div id="login" class="animate form">
                            <form  action="login.php" method="post" autocomplete="off"> 
								<input type="hidden" name="login" id="login" value="login">
                                <h1>Please enter your authentication.</h1>
								<?php if ($err != "") { ?>
								<h2 style="color:red"><?php echo $err; ?></h2>
								<?php echo '<BR>'; ?>
								<?php } ?>

								<?php if ($confirm != "") { ?>
								<h2 style="color:green"><?php echo $confirm; ?></h2>
								<?php echo '<BR>'; ?>
								<?php } ?>
                                <p> 
                                    <label for="username" class="uname" data-icon="u" > Your email or username </label>
                                    <input id="username" name="username" required="required" type="text" placeholder="myusername or mymail@mail.com"/>
                                </p>
                                <p> 
                                    <label for="password" class="youpasswd" data-icon="p"> Your password </label>
                                    <input id="password" name="password" required="required" type="password" placeholder="eg. X8df!90EO" /> 
                                </p>
                                <!--<p class="keeplogin"> 
									<input type="checkbox" name="loginkeeping" id="loginkeeping" value="loginkeeping" /> 
									<label for="loginkeeping">Keep me logged in</label>
								</p>-->
                                <p class="login button"> 
                                    <input type="submit" value="Login"/> 
								</p>
                                <p class="change_link">
									New to the team?
									<a href="#toregister" class="to_register">Join us</a>
								</p>
                            </form>
                        </div>

                        <div id="register" class="animate form">
                            <form  action="login.php" method="post" autocomplete="off">
								<input type="hidden" name="signup" id="signup" value="signup">
                                <h1> Sign up </h1> 
								<span id="msg">
								</span>
								<p> 
                                    <label for="firstnamesignup" class="fname" data-icon="u">First Name</label>
                                    <input id="firstnamesignup" name="firstnamesignup" required="required" type="text" placeholder="Daniel" value="<?php echo $fn; ?>"/>
                                </p>
								<p> 
                                    <label for="surnamesignup" class="sname" data-icon="u">SurName</label>
                                    <input id="surnamesignup" name="surnamesignup" required="required" type="text" placeholder="Michale" value="<?php echo $sn; ?>"/>
                                </p>
                                <p> 
                                    <label for="usernamesignup" class="uname" data-icon="u">Your username</label>
                                    <input id="usernamesignup" name="usernamesignup" required="required" type="text" placeholder="mysuperusername" value="<?php echo $un; ?>"/>
                                </p>
                                <p> 
                                    <label for="emailsignup" class="youmail" data-icon="e" > Your email</label>
                                    <input id="emailsignup" name="emailsignup" required="required" type="email" placeholder="mysupermail@mail.com" value="<?php echo $em; ?>"/> 
                                </p>
                                <p> 
                                    <label for="passwordsignup" class="youpasswd" data-icon="p">Your password </label>
                                    <input id="passwordsignup" name="passwordsignup" required="required" type="password" placeholder="eg. X8df!90EO"/>
                                </p>
                                <p> 
                                    <label for="passwordsignup_confirm" class="youpasswd" data-icon="p">Please confirm your password </label>
                                    <input id="passwordsignup_confirm" name="passwordsignup_confirm" required="required" type="password" placeholder="eg. X8df!90EO"/>
                                </p>
                                <p class="signin button"> 
									<input type="button" value="Sign up" onclick="return checkFields();"/> 
								</p>
                                <p class="change_link">  
									Already a member ?
									<a href="#tologin" class="to_register"> Go and log in </a>
								</p>
                            </form>
                        </div>
						
                    </div>
                </div>  
            </section>
        </div>
    </body>
</html>
