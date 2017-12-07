   
/*           
/// Whip's Auto Door/Airlock Script - revision 10.1 - 10/24/2015 ///   
/// PUBLIC RELEASE ///         
_______________________________________________________________________          
///DESCRIPTION///   
   
This script will close opened doors after 3 seconds (default).    
The duration that a door is allowed to be open can be modified lower    
down in the code (line 54). 
 
This script also supports an INFINITE number of airlock systems.        
_______________________________________________________________________         
///TIMER SETUP///       
   
Make a timer and set it to run this script and run itself every 1 second          
_______________________________________________________________________          
///AUTO DOOR CLOSER///      
   
The script will fetch ALL doors on the grid and automatically close any    
door that has been open for 3 seconds. Doors can also be excluded from    
this feature.   
   
Excluding Doors:       
    * Add the tag "[Excluded]" anywhere in the door(s) name.   
    * Airtight Hangar Doors with their default names will be excluded by default.          
_______________________________________________________________________          
///AIRLOCKS///          
   
This script supports the optional feature of airlock systems. Airlock systems    
are composed of AT LEAST one Interior Door AND one Exterior Door. The airlock   
status light does not affect the functionality of the doors so if you don't have   
space for one, don't fret :)   
   
Airlock system names should follow these patterns:   
   
    * Interior Airlock Doors: "[Prefix] Airlock Interior"   
   
    * Exterior Airlock Doors: "[Prefix] Airlock Exterior"   
   
    * Airlock Status Lights: "[Prefix] Airlock Light"   
   
You can make the prefix what ever you wish, but in order for doors in an airlock   
system to be linked by the script, they MUST have the same prefix.   
_____________________________________________________________________    
   
If you have any questions, comments, or concerns, feel free to leave a comment on           
the workshop page: http://steamcommunity.com/sharedfiles/filedetails/?id=416932930          
- Whiplash141   :)   
_____________________________________________________________________      
*/          
   
//Door open duration (in seconds); must be an integer          
    const int door_open_duration = 3;          
   
//Door exclusion string          
    string door_exclude_string = "[Excluded]";     
   
//Airlock Light Settings      
    Color alarmColor = new Color(255,0,0); //color of alarm light        
    Color regularColor = new Color (80,160,255); //color of regular light        
    Color lightColor;                
    float alarmBlinkLength = 50f;  //alarm blink length in %       
    float regularBlinkLength = 100f; //regular blink length in %       
    float lightBlinkLength;                                        
    float blinkInterval = .8f; // blink interval in seconds        
    string targetLights;      
   
//MAIN CODE BELOW: Do NOT edit if you don't know that you are doing :P   
       
    Dictionary<IMyTerminalBlock, int> dict = new Dictionary<IMyTerminalBlock, int>();   
       
void Main()           
{          
    AutoDoors();    
    Airlocks();            
}      
   
void AutoDoors()    
{          
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();              
    GridTerminalSystem.GetBlocksOfType<IMyDoor>(blocks);            
   
    for(int i = 0; i < blocks.Count; i++)     
    {          
   
    //removes Airtight Hangar Doors   
        if (blocks[i].CustomName.Contains("Airtight Hangar Door"))    
        {    
                continue;    
        }    
        else 
    //removes excluded doors   
        if (blocks[i].CustomName.Contains(door_exclude_string))    
        {    
                continue;    
        }  
        else 
        {  
            var door = blocks[i] as IMyDoor; //checks if current block is a door   
            if(dict.ContainsKey(door)) //checks dict for said door    
            {                
                if(!door.Open)      
                {          
                    dict.Remove(door); //ignores door if closed       
                }   
                else   
                {          
                    int doorCount;          
                    dict.TryGetValue(door, out doorCount); //pulls current time count       
                    dict.Remove(door); //removes old time count         
       
                    if(doorCount++ < door_open_duration)          
                    {   
                        dict.Add(door, doorCount); //adds new time count   
                    }           
                    else   
                    {   
                        door.GetActionWithName("Open_Off").Apply(door); //closes door if duration has past   
                    }                              
                }        
            }    
            else    
            {          
                if(door.Open) //if door isnt in dict, we add it at a time count of zero   
                {   
                    dict.Add(door,0);   
                }                
            }  
        } 
    }    
}    
   
void Airlocks()    
{    
    List<IMyTerminalBlock> airlockDoors = new List<IMyTerminalBlock>();    
    List<string> airlockNames = new List<string>();    
    List<string> airlockNamesNoDupes = new List<string>();    
    string airlockInteriorName;    
    string airlockExteriorName;    
    string airlockLightName;    
    string currentAirlock;   
    bool isInteriorClosed;    
    bool isExteriorClosed;     
   
//Fetch all airlock names    
    GridTerminalSystem.SearchBlocksOfName("Airlock Interior",airlockDoors); //lists all blocks with proper name    
    for (int i = 0 ; i < airlockDoors.Count ; i++)    
    {    
        if (airlockDoors[i] is IMyDoor) //checks if object is a door    
        {    
            airlockNames.Add(airlockDoors[i].CustomName);//adds name to string list    
        }    
    }    
   
//ignores duplicate names   
    for (int i = 0 ; i < airlockNames.Count ; i++)   
    {   
        if(!(airlockNamesNoDupes.Contains(airlockNames[i]))) //if we dont have this name, add it   
            airlockNamesNoDupes.Add(airlockNames[i]);   
    }   
   
//Evaluate each unique airlock name    
    for (int i = 0 ; i < airlockNamesNoDupes.Count ; i++)     
    {    
        airlockInteriorName = airlockNamesNoDupes[i]; //makes Interior door name    
        airlockExteriorName = airlockInteriorName.Replace("Interior","Exterior"); //makes Exterior door name    
        airlockLightName = airlockInteriorName.Replace("Interior","Light"); //makes Airlock Light name    
   
        List<IMyTerminalBlock> airlockInteriorList = new List<IMyTerminalBlock>();           
        List<IMyTerminalBlock> airlockExteriorList = new List<IMyTerminalBlock>();          
        GridTerminalSystem.SearchBlocksOfName(airlockInteriorName,airlockInteriorList); //get all interior doors     
        GridTerminalSystem.SearchBlocksOfName(airlockExteriorName,airlockExteriorList); //get all exterior doors     
   
    //Start checking airlock status   
        if(airlockInteriorList != null && airlockExteriorList != null) //if we have both door types    
        {          
        //fetch name of airlock group and write to console       
            currentAirlock = airlockInteriorName.Replace(" Airlock Interior","");   
            Echo ("Airlock Group '" + currentAirlock + "' found");   
   
        //we assume the airlocks are closed until proven otherwise        
            isInteriorClosed = true;    
            isExteriorClosed = true;    
                   
        //Door Interior Check    
            for(int j = 0 ; j < airlockInteriorList.Count ; j++)         
            {         
                var airlockInterior = airlockInteriorList[j] as IMyDoor;          
                   
                if(airlockInterior.Open)           
                {           
                    Lock(airlockExteriorName);      
                    LightColorChanger("alarm;" + airlockLightName);      
                    isInteriorClosed = false;     
                        //if any doors yield false, bool will persist until comparison    
                }         
            }        
   
        //Door Exterior Check           
            for(int j = 0 ; j < airlockExteriorList.Count ; j++)         
            {         
                var airlockExterior = airlockExteriorList[j] as IMyDoor;   
                  
                if(airlockExterior.Open)           
                {           
                    Lock(airlockInteriorName);       
                    LightColorChanger("alarm;" + airlockLightName);      
                    isExteriorClosed = false;    
                }         
            }       
   
        //if all Interior & Exterior doors closed     
            if(isInteriorClosed == true && isExteriorClosed == true)     
            {     
                LightColorChanger("regular;" + airlockLightName);     
            }     
   
        //if all Interior doors closed     
            if(isInteriorClosed == true)       
            {       
                Unlock(airlockExteriorName);        
            }       
   
        //if all Exterior doors closed     
            if(isExteriorClosed == true)       
            {       
                Unlock(airlockInteriorName);        
            }           
        }    
    }   
}       
   
void Lock(string lockName)         
{         
//locks all doors with the inputed name       
    List<IMyTerminalBlock> lock_door_list = new List<IMyTerminalBlock>();         
    GridTerminalSystem.SearchBlocksOfName(lockName,lock_door_list);         
             
    for(int i = 0 ; i < lock_door_list.Count ; i++)         
    {         
        IMyDoor lock_door = lock_door_list[i] as IMyDoor;         
        lock_door.ApplyAction("Open_Off");          
        lock_door.ApplyAction("OnOff_Off");         
    }         
    Echo(lockName + " is locked");         
}         
   
void Unlock(string unlockName)         
{      
//unlocks all doors with inputed name      
    List<IMyTerminalBlock> unlock_door_list = new List<IMyTerminalBlock>();         
    GridTerminalSystem.SearchBlocksOfName(unlockName,unlock_door_list);         
             
    for(int i = 0 ; i < unlock_door_list.Count ; i++)         
    {         
        IMyDoor unlock_door = unlock_door_list[i] as IMyDoor;         
        unlock_door.ApplyAction("OnOff_On");         
    }         
    Echo(unlockName + " is unlocked");        
}      
   
void LightColorChanger(string cmd)        
{         
//applies our status colors to the airlock lights   
    string[] cmd_split = cmd.Split(';');         
    targetLights = cmd_split[1]; //2nd element is name        
        
    switch(cmd_split[0].ToLower()) //1st element is command        
    {        
        case "alarm":        
            lightColor = alarmColor;        
            lightBlinkLength = alarmBlinkLength;       
            break;        
                    
        case "regular":            
            lightColor = regularColor;       
            lightBlinkLength = regularBlinkLength;           
            break;        
                    
        default:        
            break;        
    }        
   
    List<IMyTerminalBlock> Lights = new List<IMyTerminalBlock>();              
    GridTerminalSystem.SearchBlocksOfName(targetLights,Lights);         
    for( int i=0 ; i< Lights.Count ; i++)                       
    {        
        var Light = Lights[i] as IMyLightingBlock;                                             
        Light.SetValue("Color",lightColor);                 
        Light.SetValue("Blink Lenght",lightBlinkLength);                 
        Light.SetValue("Blink Interval",blinkInterval);                 
    }            
}            
