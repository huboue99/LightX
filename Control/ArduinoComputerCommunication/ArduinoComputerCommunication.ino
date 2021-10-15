// Source pour l'inspiration du code: https://www.instructables.com/How-to-Connect-I2C-Lcd-Display-to-Arduino-Uno/
// Version 0 : 2021-04-08
// Cassandra Beaupré  
// Gabrielle Paquette

#include <Wire.h> 
#include <LiquidCrystal_I2C.h>

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
String commandString = "";

LiquidCrystal_I2C lcd(0x27, 2, 1, 0, 4, 5, 6, 7, 3, POSITIVE); 

boolean isConnected = false;

void setup() {
  Serial.begin(9600);
  initDisplay();
}

void loop() {

if(stringComplete)
{
  stringComplete = false;
  getCommand();
  
  if(commandString.equals("STAR"))
  {
    lcd.clear();
    lcd.print("Ready to connect"); 
  }
  else if(commandString.equals("TEXT"))
  {
    String text = getTextToPrint();
    printText(text);
  }
  inputString = "";
}
}

void initDisplay()
{
  lcd.begin(16, 2);
  lcd.print("Ready to connect");
  lcd.backlight();
}

void getCommand()
{
  if(inputString.length()>0)
  {
     commandString = inputString.substring(1,5);
  }
}

String getTextToPrint()
{
  String value = inputString.substring(5,inputString.length()-2);
  return value;
}

void printText(String text)
{
  lcd.clear();
  lcd.setCursor(0,0);
    if(text.length()<16)
    {
      lcd.print(text);
    }else
    {
      lcd.print(text.substring(0,16));
      lcd.setCursor(0,1);
      lcd.print(text.substring(16,32));
    }
}

void serialEvent() {
  while (Serial.available()) {
    char inChar = (char)Serial.read();
    inputString += inChar;
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}
