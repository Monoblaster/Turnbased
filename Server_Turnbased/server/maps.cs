//makes a singleton object to hold inital data
function server_CreateMap(%name,%brickData,%description,%values)
{
    //might want to consider loading bricks here
    %m = new ScriptObject("TBM" @ %name)
    {
        class = "TurnbasedMap";

        pieceName = %name;
        brickData = %brickData;
    };

    $Turnabsed::Server::MapDataGroup.add(%m);
    //maybe cache the 2d map array here
}

function TurnbasedMap::NewMap(%map,%client,%vector)
{
    //map storage object
    %m = new ScriptObject(){};
    %xmin = inf;
    %ymin = inf;
    //create bricks
    %brickData = %map.brickData;

    %brickList = turnbasedLoad(%brickData,%vector,%rotation);
    %m.brickList = %brickList;

    %count = %bricklist.getCount();
    for(%c = 0; %c < %count; %c++)
    {
        %brick = %brickList.getObject(%c);
        %brickName = $turnbasedNameBrick[%brick];

        %brick.isPlanted = true;

        %brick.plant();
        %brick.setTrusted(true);

        %brick.map = %m;

        if(%brickName !$= "")
        {
            //add it as a map
            %position = %brick.position;
            %x = getWord(%position,0);
            %y = getWord(%position,1);

            if(%xmin > %x)
                %xmin = %x;

            if(%ymin > %y)
                %ymin = %y;
        }
    }
    for(%c = 0; %c < %count; %c++)
    {
        %brick = %brickList.getObject(%c);
        %brickName = $turnbasedNameBrick[%brick];

        if(%brickName !$= "")
        {
            //add it as a map
            %position = %brick.position;
            %position = vectorScale(vectorSub(%position,%xmin SPC %ymin SPC "0"),2);

            %m.map[getWord(%position,0), getWord(%position,1)] = %brick;
        }
    }
}