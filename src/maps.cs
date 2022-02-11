//makes a singleton object to hold inital data
function server_CreateMap(%name,%brickData,%description,%values)
{
    //removing previous instances
    if(isObject("TBM" @ %name))
        ("TBM" @ %name).delete();
    
    %m = new ScriptObject("TBM" @ %name)
    {
        class = "TurnbasedMap";

        pieceName = %name;
        brickData = %brickData;
    };
    //bricks are chached into global variables
    turnbasedCache(%brickData);
    //maybe cache the 2d map array here
    %m.mapArray[0,0] = 0;
    //use cached version to figure a corner and then values for bricks
    %c = 0;
    %xmin = inf;
    %ymin = inf;
    while((%pos = $TBposition[%brickData,%c]) !$= "")
    {
        if($TBname[%brickData,%c] !$= "")
        {
            %x = getWord(%pos,0);
            %y = getWord(%pos,1);

            if(%xmin > %x)
                %xmin = %x;

            if(%ymin > %y)
                %ymin = %y;
        }
        %c++;
    }
    //caching the map values
    for(%i = 0; %i < %c; %i++)
    {
        if($TBname[%brickData,%i] !$= "")
        {
            %position = $TBposition[%brickData,%i];
            %position = vectorScale(vectorSub(%position,%xmin SPC %ymin SPC "0"),2);

            $TBmapPosition[%brickData,%i] = getWord(%position,0) @ "," @ getword(%position,1);
        }
    }

    $Turnabsed::Server::MapDataGroup.add(%m);
}

function TurnbasedMap::NewMap(%map,%vector)
{
    //map storage object
    %m = new ScriptObject(){};
    //create bricks
    %brickData = %map.brickData;

    %brickList = turnbasedLoad(%brickData,%vector,%rotation);
    %m.brickList = %brickList;

    %count = %bricklist.getCount();
    for(%i = 0; %i < %count; %i++)
    {
        %brick = %brickList.getObject(%i);
        %brickName = $turnbasedNameBrick[%brick];

        %brick.isPlanted = true;

        %brick.plant();
        %brick.setTrusted(true);

        %brick.map = %m;

        if(%brickName !$= "")
            %m.mapPosition[$TBmapPosition[%brickData,%i]] = %brick;
    }
    return %m;
}