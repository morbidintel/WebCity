<?php
   $to = "zaihao90@hotmail.com"; // <– replace with your address here
   $subject = "Test mail";
   $message = "Hello! This is a simple test email message.";
   $from = "webcity.online@webcity.online";
   $headers = "From:" . $from;
   mail($to,$subject,$message,$headers);
   echo "Mail Sent.";
?>