#include <Arduino.h>
#include <Tic.h>

TicI2C ticX(14);
TicI2C ticY(15);
TicI2C ticZ(16);
TicI2C ticT(17);

const int N_MOTORS = 4 ;
const int n = 2 ;           // pour ne pas utiliser Z et theta 

int resetPin = 4;

const int X_microStep = 1 ; //1 pour du full step, 2 pour half, 4 pour 1/4, etc.
const int Y_microStep = 1 ;
const int Z_microStep = 1 ;
const int T_microStep = 1 ;

bool isReset = false; 

int commandeMax[N_MOTORS]{
  2500*X_microStep,
  2500*Y_microStep,
  325 *Z_microStep,
  50  *T_microStep
};

int commandeMin[N_MOTORS]{
  -2500*X_microStep,
  -2500*Y_microStep,
   0   *Z_microStep,
  -20  *T_microStep
};

void waitForHoming(TicI2C tic){
  do
  {;
  } while(tic.getHomingActive());
}

void waitForPosition(TicI2C tic, int32_t targetPosition){
  do
  {
  } while (tic.getCurrentPosition() != targetPosition);
}

void setup() {
  Wire.begin();
  Serial.begin(57600);
  //resetfunc();
}

//void(* resetFunc) (void) = 0;
void resetFunc()
{
  
  ticX.exitSafeStart();
  ticY.exitSafeStart();
  ticZ.exitSafeStart();
  ticT.exitSafeStart();
  
  ticX.goHomeReverse();
  ticY.goHomeReverse();
  ticZ.goHomeReverse();
  //ticT.goHomeReverse();
  
  waitForHoming(ticX);
  waitForHoming(ticY);
  waitForHoming(ticZ);
  //waitForHoming(ticT);
  
  ticX.haltAndSetPosition(0);
  ticY.haltAndSetPosition(0);
  ticZ.haltAndSetPosition(0);
  ticT.haltAndSetPosition(0);
  
  ticX.setTargetPosition(2500);
  ticY.setTargetPosition(2500);
  //ticT.setTargetPosition(50);

  waitForPosition(ticX, 2500);
  waitForPosition(ticY, 2500);
  //waitForPosition(ticZ);
  //waitForPosition(ticT,50);

  ticX.haltAndSetPosition(0);
  ticY.haltAndSetPosition(0);
  //ticZ.haltAndSetPosition(0);
  //ticT.haltAndSetPosition(0);
 }
void treatI2CCommand(uint8_t buffer[6]){
  if (!(ticX.getEnergized()&& ticY.getEnergized()&& ticT.getEnergized()))//Modif
  {
    ticX.exitSafeStart();
    ticY.exitSafeStart();
    ticZ.exitSafeStart();
    ticT.exitSafeStart();
  }
  TicI2C* currentDriver=nullptr;
  //on vérifie l'adresse
  int indiceCurrentDriver=-1;
  switch (buffer[0])
  {
  case 14://ticX.getAddress():
    currentDriver=&ticX;
    indiceCurrentDriver=0;
    break;
  case 15://ticY.getAddress():
    currentDriver=&ticY;
    indiceCurrentDriver=1;
    break;
  case 16://ticZ.getAddress():
    currentDriver=&ticZ;
    indiceCurrentDriver=2;
    break;
  case 17://ticT.getAddress():
    currentDriver=&ticT;
    indiceCurrentDriver=3;
    break;
  default:
    return;
    break;
  }
  //on accumule le data contenu dans les 4 octets
  int32_t data=0;
  for (int i = 0; i < 4; i++)
    {
      data |=((int(buffer[i+2]))<<(8*i));
    }
  
  //On applique la commande
  switch (TicCommand(buffer[1]&0xFF))
  {
  case TicCommand::SetTargetPosition:{
    int32_t newPosition=currentDriver->getCurrentPosition();
    newPosition = newPosition + data;
    //On vérifie qu'on ne sort pas des bornes
    if(newPosition>commandeMax[indiceCurrentDriver])
       newPosition =commandeMax[indiceCurrentDriver];
    else if(newPosition <commandeMin[indiceCurrentDriver])
       newPosition =commandeMin[indiceCurrentDriver];
    currentDriver->setTargetPosition(newPosition);
    break;}
  case TicCommand::SetTargetVelocity:{
    int32_t vitesse = 10000*data;
    currentDriver->setTargetVelocity(vitesse);
    break;}
  case TicCommand::GetVariable:{
    int newData;
    newData = currentDriver->getCurrentPosition();
	uint8_t buf[4]={};
	for(int i=0; i<4;i++){
		buf[i]=(newData>>8*i)&0xFF;
	}
    Serial.write(buf,4);
    break;}
  case TicCommand::SetStartingSpeed:{
    resetFunc();
    break;}
  default:{
      return;
    break;}
  }

  Serial.flush();
}


void loop() {  
  uint8_t buffer[6]={};
  if(Serial.available()>=6){
    Serial.readBytes(buffer, 6);
    treatI2CCommand(buffer);
    Serial.flush();
  }
}
