<?php

class Encryption {
    private $key, $iv;
    function __construct() {
        $this->key = "edrtjfjfjlldldld";
        $this->iv = "56666852251557009888889955123458";
    }
    function encrypt($text) {

        $block = mcrypt_get_block_size(MCRYPT_RIJNDAEL_256, MCRYPT_MODE_CBC);
        $padding = $block - (strlen($text) % $block);
        $text .= str_repeat(chr($padding), $padding);
        $crypttext = mcrypt_encrypt(MCRYPT_RIJNDAEL_256, $this->key, $text, MCRYPT_MODE_CBC, $this->iv);

        return base64_encode($crypttext);
    }

    function decrypt($input) {
        $dectext = mcrypt_decrypt(MCRYPT_RIJNDAEL_256, $this->key, base64_decode($input), MCRYPT_MODE_CBC, $this->iv);
        return $dectext;
    }
}