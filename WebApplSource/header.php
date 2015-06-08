<?php
?>

<div id="heading" style="height:100px; background-color:white;"> 
	<div id="heading_container" style="max-width:100%;min-width:100%;height:180px;background: url('images/Kensee_Logo.png') no-repeat; background-size:95px auto; background-position:50px 20px;">
		
		<div id="block1" style="margin-left:50px;height:100px;">
			<div id="block2" style="padding-top:55px;">
				<div id="menu" style="border-bottom:0; width:auto; float:right; padding-left:600px;">
					<a href="<?php echo $SITEURL; ?>/index.php">Home</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
					<?php if (isset($_SESSION['ADMIN']) and $_SESSION['ADMIN'] == "1") { ?>
					<a href="<?php echo $SITEURL; ?>/site.php"><img src="<?php echo $SITEURL; ?>/img/setting.png" style="width:25px; height:auto;"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
					<?php } ?>
					<?php if (isset($_SESSION['USER_ID']) and $_SESSION['USER_ID'] != "") { ?>
					<a href="<?php echo $SITEURL; ?>/logout.php">Log Out</a>
					<?php } ?>
				</div>
				<!--<div id="social_links" style="width:auto; position:absolute; margin-right:5px; padding-top:55px;">
					<a href="javascript:void(0);" target="_blank" id="top_twitter" style="float:right">Twitter</a>
					<a href="javascript:void(0);" target="_blank" id="top_facebook" style="float:right">Facebook</a>
				</div>-->

				<div class="clear"></div>

			</div> <!-- block2 -->
		</div>					 
	</div>

	<div class="clear"></div>
</div> <!-- HEADING -->