//format
//brick count
//bricks on seperate lines
//identifiers (name, list of brick numbers)
function turnbasedSaveFromDup(%client,%filename)
{
    if($ns[%client.ndSelection,"B",0] $= "")
        return;

    %lineCount = -1;
    %c = 0;
    %nameCount = 0;

    %selection = %client.ndSelection;

    //put bricks into a list
    while($ns[%client.ndSelection,"B",%c] !$= "")
    {
        //brick information
        %line[%lineCount++] = $ns[%selection,"D",%c].getName() TAB $ns[%selection,"P",%c] TAB $ns[%selection,"R",%c] TAB $ns[%selection,"CO",%c] TAB $ns[%selection,"CF",%c] TAB $ns[%selection,"SF",%c] TAB $ns[%selection,"PR",%c];
        
        //add names to a list for later
        if((%name = $NS[%selection, "NT", %c]) !$= "")
        {
            if((%nameNumber = %nameExists[%name]) $= "")
            {
                %nameExists[%name] = %nameCount;
                %name[%nameCount] = %name TAB %lineCount;
                %nameCount++;
            }
            else
            {
                %name[%nameNumber] = %name[%nameNumber] SPC %lineCount;
            }
        }

        %c++;
    }

    %file = new FileObject(){};
    %file.openForWrite("config/server/TurnbasedSaves/" @ %filename @ ".txt");
    //count
    %file.writeLine(%c - 1);
    //write bricks
    for(%i = 0; %i <= %lineCount; %i++)
    {
        %file.writeLine(%line[%i]);
    }
    for(%i = 0; %i < %nameCount; %i++)
    {
        %file.writeLine(%name[%i]);
    }
    //write identifiers
    %file.close();
    %file.delete();
}
function turnbasedCache(%fileName)
{
    %file = new FileObject(){};
    %success = %file.openForRead("config/server/TurnbasedSaves/" @ %filename @ ".txt");

    if(!%success)
        return;

    %c = 0;
    %count = %file.readLine();
    //read in bricks
    while(%c <= %count)
    {
        %line = %file.readLine();
        $TBdatablock[%fileName,%c] = getField(%line,0);
        $TBposition[%fileName,%c] = getField(%line,1);
        $TBrotation[%fileName,%c] = getField(%line,2);
        $TBcolorid[%fileName,%c] = getField(%line,3);
        $TBcolorfx[%fileName,%c] = getField(%line,4);
        $TBshapefx[%fileName,%c] = getField(%line,5);
        $TBprint[%fileName,%c] = getfield(%line,6);

        %c++;
    }
    //read named brick info into a list to be reference later
    %nameCount = 0;
    while(!%file.isEOF())
    {
        %line = %file.readLine();
        %name = getField(%line,0);
        %list = getField(%line,1);

        %c = 0;
        while((%number = getWord(%list,%c)) !$= "")
        {
            $TBname[%filename,%number] = %name;

            %c++;
        }
    }
}
function turnbasedCachLoad(%filename,%vector,%rotation)
{
    talk("loading from cache");

    //we want to store the bricks in a list for a return
    %brickList = new SimSet(){};
    //create bricks
    %c = 0;
    while($TBdatablock[%fileName,%c] !$= "")
    {
        %brick = new fxDTSBrick()
        {
            dataBlock = $TBdatablock[%fileName,%c].getId();
            position = vectorAdd($TBposition[%fileName,%c],%vector); //add in our vector
            rotation = $TBrotation[%fileName,%c];
            colorid = $TBcolorid[%fileName,%c];
            colorfx = $TBcolorfx[%fileName,%c];
            shapefx = $TBshapefx[%fileName,%c];
            print = $TBprint[%fileName,%c];
            // isPlanted = true;
        };
        // %brick.plant();
        // %brick.setTrusted(true);

        %brickList.add(%brick);

        //global named brick list whatever for easyness
        $turnbasedNameBrick[%brick] = $TBname[%fileName,%c];

        //999999
        mainBrickGroup.getObject(1).add(%brick);
        %c++;
    }
    return %brickList;
}
function turnbasedLoad(%fileName,%vector,%rotation)
{
    //load from the cache if it's availible
    if($TBdatablock[%fileName,0] !$= "")
        return turnbasedCachLoad(%fileName,%vector,%rotation);

    %file = new FileObject(){};
    %success = %file.openForRead("config/server/TurnbasedSaves/" @ %filename @ ".txt");

    if(!%success)
        return;

    %c = 0;
    %count = %file.readLine();
    //read in bricks
    while(%c <= %count)
    {
        %line = %file.readLine();
        %datablock[%c] = getField(%line,0);
        %position[%c] = getField(%line,1);
        %rotation[%c] = getField(%line,2);
        %colorid[%c] = getField(%line,3);
        %colorfx[%c] = getField(%line,4);
        %shapefx[%c] = getField(%line,5);
        %print[%c] = getfield(%line,6);

        %c++;
    }
    //read named brick info into a list to be reference later
    %nameCount = 0;
    while(!%file.isEOF())
    {
        %line = %file.readLine();
        %name = getField(%line,0);
        %list = getField(%line,1);

        %c = 0;
        while((%number = getWord(%list,%c)) !$= "")
        {
            %name[%number] = %name;

            %c++;
        }
    }
    //we want to store the bricks in a list for a return
    %brickList = new SimSet(){};
    //create bricks
    %c = 0;
    while(%datablock[%c] !$= "")
    {
        %brick = new fxDTSBrick()
        {
            dataBlock = %datablock[%c].getId();
            position = vectorAdd(%position[%c],%vector); //add in our vector
            rotation = %rotation[%c];
            colorid = %colorid[%c];
            colorfx = %colorfx[%c];
            shapefx = %shapefx[%c];
            print = %print[%c];
            // isPlanted = true;
        };
        // %brick.plant();
        // %brick.setTrusted(true);

        %brickList.add(%brick);

        //global named brick list whatever for easyness
        $turnbasedNameBrick[%brick] = %name[%c];

        //999999
        mainBrickGroup.getObject(1).add(%brick);
        %c++;
    }
    return %brickList;
}
function serverCmdTurnbasedSave(%client,%filename)
{
    talk("balrg");
}
function serverCmdTurnbasedLoad(%client,%filename, %x, %y, %z, %rotation)
{
    talk("arag");
}