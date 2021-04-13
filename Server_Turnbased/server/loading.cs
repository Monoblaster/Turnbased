//Credit to Zeblote and New Duplicator add-on for this.

function TurnbasedLoader::startLoading(%this, %filePath)
{
	//Open file
	%this.loadFile = new FileObject();

	if(!%this.loadFile.openForRead(%filePath))
		return false;

	//Skip file header
	%this.loadFile.readLine();
	%cnt = %this.loadFile.readLine();

	for(%i = 0; %i < %cnt; %i++)
		%this.loadFile.readLine();

	//Read colorset
	for(%i = 0; %i < 64; %i++)
		$NS[%this, "CT", %i] = ndGetClosestColorID2(getColorI(%this.loadFile.readLine()));

	//Read line count (temporary, allows displaying percentage)
	%this.loadExpectedBrickCount = getWord(%this.loadFile.readLine(), 1) * 1;

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	//Schedule first tick
	%this.connectionCount = 0;
	%this.brickCount = 0;
	%this.loadCount = 0;

	%this.loadStage = 0;
	%this.loadIndex = -1;
	%this.loadSchedule = %this.schedule(30, tickLoadBricks);

	return true;
}

function TurnbasedLoader::tickLoadBricks(%this)
{
	cancel(%this.loadSchedule);

	%file = %this.loadFile;
	%index = %this.loadIndex;

	%loadCount = %this.loadCount;

	//Process lines
	while(!%file.isEOF())
	{
		%line = %file.readLine();

		//Skip empty lines
		if(trim(%line $= ""))
			continue;

		//Figure out what to do with the line
		switch$(getWord(%line, 0))
		{
			//Line is brick name
			case "+-NTOBJECTNAME":

				$NS[%this, "NT", %index] = getWord(%line, 1);

			//Line is event
			case "+-EVENT":

				//Mostly copied from default loading code
				%idx = $NS[%this, "EN", %index];

				if(!%idx)
					%idx = 0;

				%enabled = getField(%line, 2);
				%inputName = getField(%line, 3);
				%delay = getField(%line, 4);
				%targetName = getField(%line, 5);
				%NT = getField(%line, 6);
				%outputName = getField(%line, 7);
				%par1 = getField(%line, 8);
				%par2 = getField(%line, 9);
				%par3 = getField(%line, 10);
				%par4 = getField(%line, 11);

				%inputIdx = inputEvent_GetInputEventIdx(%inputName);

				if(%inputIdx == -1)
					warn("LOAD DUP: Input Event not found for name \"" @ %inputName @ "\"");

				%targetIdx = inputEvent_GetTargetIndex("FxDTSBrick", %inputIdx, %targetName);

				if(%targetName == -1)
					%targetClass = "FxDTSBrick";
				else
				{
					%field = getField($InputEvent_TargetList["FxDTSBrick", %inputIdx], %targetIdx);
					%targetClass = getWord(%field, 1);
				}

				%outputIdx = outputEvent_GetOutputEventIdx(%targetClass, %outputName);

				if(%outputIdx == -1)
					warn("LOAD DUP: Output Event not found for name \"" @ %outputName @ "\"");

				for(%j = 1; %j < 5; %j++)
				{
					%field = getField($OutputEvent_ParameterList[%targetClass, %outputIdx], %j - 1);
					%dataType = getWord(%field, 0);

					if(%dataType $= "Datablock" && %par[%j] !$= "-1")
					{
						%par[%j] = nameToId(%par[%j]);

						if(!isObject(%par[%j]))
						{
							warn("LOAD DUP: Datablock not found for event " @ %outputName @ " -> " @ %par[%j]);
							%par[%j] = 0;
						}
					}
				}

				//Save event
				$NS[%this, "EE", %index, %idx] = %enabled;
				$NS[%this, "ED", %index, %idx] = %delay;

				$NS[%this, "EI", %index, %idx] = %inputName;
				$NS[%this, "EII", %index, %idx] = %inputIdx;

				$NS[%this, "EO", %index, %idx] = %outputName;
				$NS[%this, "EOI", %index, %idx] = %outputIdx;
				$NS[%this, "EOC", %index, %idx] = $OutputEvent_AppendClient["FxDTSBrick", %outputIdx];

				$NS[%this, "ET", %index, %idx] = %targetName;
				$NS[%this, "ETI", %index, %idx] = %targetIdx;
				$NS[%this, "ENT", %index, %idx] = %NT;

				$NS[%this, "EP", %index, %idx, 0] = %par1;
				$NS[%this, "EP", %index, %idx, 1] = %par2;
				$NS[%this, "EP", %index, %idx, 2] = %par3;
				$NS[%this, "EP", %index, %idx, 3] = %par4;

				$NS[%this, "EN", %index] = %idx + 1;

			//Line is emitter
			case "+-EMITTER":

				%line = getSubStr(%line, 10, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				if(%dbName !$= "NONE")
				{
					%db = $UINameTable_Emitters[%dbName];

					//Ensure emitter exists
					if(!isObject(%db))
					{
						warn("LOAD DUP: Emitter datablock no found for uiName \"" @ %dbName @ "\"");
						%db = 0;
					}
				}
				else
					%db = 0;

				$NS[%this, "ED", %index] = %db;
				$NS[%this, "ER", %index] = mFLoor(getSubStr(%line, %pos + 2, 9999));

			//Line is light
			case "+-LIGHT":

				%line = getSubStr(%line, 8, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				%db = $UINameTable_Lights[%dbName];

				//Ensure light exists
				if(!isObject(%db))
				{
					warn("LOAD DUP: Light datablock no found for uiName \"" @ %dbName @ "\"");
					%db = 0;
				}
				else
					$NS[%this, "LD", %index] = %db;

			//Line is item
			case "+-ITEM":

				%line = getSubStr(%line, 7, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				if(%dbName !$= "NONE")
				{
					%db = $UINameTable_Items[%dbName];

					//Ensure item exists
					if(!isObject(%db))
					{
						warn("LOAD DUP: Item datablock no found for uiName \"" @ %dbName @ "\"");
						%db = 0;
					}
				}
				else
					%db = 0;

				%line = getSubStr(%line, %pos + 2, 9999);

				$NS[%this, "ID", %index] = %db;
				$NS[%this, "IP", %index] = getWord(%line, 0);
				$NS[%this, "IR", %index] = getWord(%line, 1);
				$NS[%this, "IT", %index] = getWord(%line, 2);

			//Line is music
			case "+-AUDIOEMITTER":

				%line = getSubStr(%line, 15, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				%db = $UINameTable_Music[%dbName];

				//Ensure music exists
				if(!isObject(%db))
				{
					warn("LOAD DUP: Music datablock no found for uiName \"" @ %dbName @ "\"");
					%db = 0;
				}
				else
					$NS[%this, "MD", %index] = %db;

			//Line is vehicle
			case "+-VEHICLE":

				%line = getSubStr(%line, 10, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				if(%dbName !$= "NONE")
				{
					%db = $UINameTable_Vehicle[%dbName];

					//Ensure vehicle exists
					if(!isObject(%db))
					{
						warn("LOAD DUP: Vehicle datablock no found for uiName \"" @ %dbName @ "\"");
						%db = 0;
					}
				}
				else
					%db = 0;

				$NS[%this, "VD", %index] = %db;
				$NS[%this, "VC", %index] = mFLoor(getSubStr(%line, %pos + 2, 9999));

			//Start reading connections
			case "ND_SIZE\"":

				%version = getWord(%line, 1);
				%this.loadExpectedConnectionCount = getWord(%line, 2);
				%numberSize = getWord(%line, 3);
				%indexSize = getWord(%line, 4);
				%connections = true;
				break;

			//Error
			case "ND_TREE\"":

				warn("LOAD DUP: Got connection data before connection sizes");

			//Line is irrelevant
			case "+-OWNER":

				%nothing = "";

			//Line is brick
			default:

				//Increment selection index
				%index++;
				%quotePos = strstr(%line, "\"");

				if(%quotePos >= 0)
				{
					//Get datablock
					%uiName = getSubStr(%line, 0, %quotePos);
					%db = $uiNameTable[%uiName];

					if(isObject(%db))
					{
						$NS[%this, "D", %index] = %db;

						//Load all the info from brick line
						%line = getSubStr(%line, %quotePos + 2, 9999);
						%pos = getWords(%line, 0, 2);
						%angId = getWord(%line, 3);

						if(%loadCount == 0)
							%this.rootPosition = %pos;

						$NS[%this, "P", %index] = vectorSub(%pos, %this.rootPosition);
						$NS[%this, "R", %index] = %angId;

						$NS[%this, "CO", %index] = $NS[%this, "CT", getWord(%line, 5)];
						$NS[%this, "CF", %index] = getWord(%line, 7);
						$NS[%this, "SF", %index] = getWord(%line, 8);

						if(%db.hasPrint)
						{
							if((%print = $printNameTable[getWord(%line, 6)]) $= "")
								warn("LOAD DUP: Print texture not found for path \"" @ getWord(%line, 6) @ "\"");

							$NS[%this, "PR", %index] = %print;
						}

						if(!getWord(%line, 9))
							$NS[%this, "NRC", %index] = true;

						if(!getWord(%line, 10))
							$NS[%this, "NC", %index] = true;

						if(!getWord(%line, 11))
							$NS[%this, "NR", %index] = true;

						//Update selection size with brick datablock
						if(%angId % 2 == 0)
						{
							%sx = %db.brickSizeX / 4;
							%sy = %db.brickSizeY / 4;
						}
						else
						{
							%sy = %db.brickSizeX / 4;
							%sx = %db.brickSizeY / 4;
						}

						%sz = %db.brickSizeZ / 10;

						%minX = getWord(%pos, 0) - %sx;
						%minY = getWord(%pos, 1) - %sy;
						%minZ = getWord(%pos, 2) - %sz;
						%maxX = getWord(%pos, 0) + %sx;
						%maxY = getWord(%pos, 1) + %sy;
						%maxZ = getWord(%pos, 2) + %sz;

						if(%loadCount)
						{
							if(%minX < %this.minX)
								%this.minX = %minX;

							if(%minY < %this.minY)
								%this.minY = %minY;

							if(%minZ < %this.minZ)
								%this.minZ = %minZ;

							if(%maxX > %this.maxX)
								%this.maxX = %maxX;

							if(%maxY > %this.maxY)
								%this.maxY = %maxY;

							if(%maxZ > %this.maxZ)
								%this.maxZ = %maxZ;
						}
						else
						{
							%this.minX = %minX;
							%this.minY = %minY;
							%this.minZ = %minZ;
							%this.maxX = %maxX;
							%this.maxY = %maxY;
							%this.maxZ = %maxZ;
						}

						%loadCount++;
					}
					else
					{
						warn("LOAD DUP: Brick datablock not found for uiName \"" @ %uiName @ "\"");
						$NS[%this, "D", %index] = 0;
					}
				}
				else
				{
					warn("LOAD DUP: Brick uiName missing on line \"" @ %line @ "\"");
					$NS[%this, "D", %index] = 0;
				}
		}

		if(%linesProcessed++ > $Pref::Server::ND::ProcessPerTick * 2)
			break;
	}
	//Save how far we got
	%this.loadIndex = %index;
	%this.brickCount = %index + 1;
	%this.loadCount = %loadCount;

	//Tell the client how much we loaded this tick
	// if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	// {
	// 	%this.client.ndUpdateBottomPrint();
	// 	%this.client.ndLastMessageTime = $Sim::Time;
	// }

	//Switch over to connection mode if necessary
	if(%connections)
	{
		%this.loadStage = 1;
		%this.loadIndex = 0;
		%this.connectionCount = 0;
		%this.connectionIndex = -1;
		%this.connectionIndex2 = 0;
		%this.connectionsRemaining = 0;

		if((%numberSize != 1 && %numberSize != 2 && %numberSize != 3) ||
		    (%indexSize != 1 &&  %indexSize != 2 &&  %indexSize != 3))
		{
			messageClient(%this.client, '', "\c0Warning:\c6 The connection data is corrupted. Planting may not work as expected.");
			%this.finishLoading();
			return;
		}

		//Create byte table
		if(!$ND::Byte241TableCreated)
			ndCreateByte241Table();
		%this.loadSchedule = %this.schedule(30, tickLoadConnections, %numberSize, %indexSize);
		return;
	}
	//Reached end of file, means we got no connection data
	if(%file.isEOF())
	{
		messageClient(%this.client, '', "\c0Warning:\c6 The save was not written by the New Duplicator. Planting may not work as expected.");
		%this.finishLoading();
	}
	else
		%this.loadSchedule = %this.schedule(30, tickLoadBricks);
}

//Load connections
function TurnbasedLoader::tickLoadConnections(%this, %numberSize, %indexSize)
{
	cancel(%this.loadSchedule);

	%connections = %this.connectionCount;
	%maxConnections = %this.maxConnections;
	%connectionIndex = %this.connectionIndex;
	%connectionIndex2 = %this.connectionIndex2;
	%connectionsRemaining = %this.connectionsRemaining;

	//Process 10 lines
	for(%i = 0; %i < 10 && !%this.loadFile.isEOF(); %i++)
	{
		%line = getSubStr(%this.loadFile.readLine(), 9, 9999);
		%len = strLen(%line);
		%pos = 0;

		while(%pos < %len)
		{
			if(%connectionsRemaining)
			{
				//Read a connection
				if(%indexSize == 1)
				{
					$NS[%this, "C", %connectionIndex, %connectionIndex2] =
					    strStr($ND::Byte241Lookup, getSubStr(%line, %pos, 1));

					%pos++;
				}
				else if(%indexSize == 2)
				{
					%tmp = getSubStr(%line, %pos, 2);

					$NS[%this, "C", %connectionIndex, %connectionIndex2] =
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 241 +
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1));

					%pos += 2;
				}
				else
				{
					%tmp = getSubStr(%line, %pos, 3);

					$NS[%this, "C", %connectionIndex, %connectionIndex2] =
					    ((strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 58081) | 0) +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1)) *   241       +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 2, 1));

					%pos += 3;
				}

				%connectionsRemaining--;
				%connectionIndex2++;
				%connections++;
			}
			else
			{
				//No connections remaining for active brick, increment index
				%connectionIndex++;
				%connectionIndex2 = 0;

				//Read a connection number
				if(%numberSize == 1)
				{
					%connectionsRemaining =
					    strStr($ND::Byte241Lookup, getSubStr(%line, %pos, 1));

					%pos++;
				}
				else if(%numberSize == 2)
				{
					%tmp = getSubStr(%line, %pos, 2);

					%connectionsRemaining =
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 241 +
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1));

					%pos += 2;
				}
				else
				{
					%tmp = getSubStr(%line, %pos, 3);

					%connectionsRemaining =
					    ((strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 58081) | 0) +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1)) *   241       +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 2, 1));

					%pos += 3;
				}

				$NS[%this, "N", %connectionIndex] = %connectionsRemaining;

				if(%maxConnections < %connectionsRemaining)
					%maxConnections = %connectionsRemaining;
			}
		}
	}

	//Save how far we got
	%this.connectionCount = %connections;
	%this.maxConnections = %maxConnections;
	%this.connectionIndex = %connectionIndex;
	%this.connectionIndex2 = %connectionIndex2;
	%this.connectionsRemaining = %connectionsRemaining;

	//Tell the client how much we loaded this tick
	// if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	// {
	// 	%this.client.ndUpdateBottomPrint();
	// 	%this.client.ndLastMessageTime = $Sim::Time;
	// }
	//Check if we're done
	if(%this.loadFile.isEOF())
		%this.finishLoading();
	else
		%this.loadSchedule = %this.schedule(30, tickLoadConnections, %numberSize, %indexSize);
}

//Finish loading
function TurnbasedLoader::finishLoading(%this)
{
	%this.loadFile.close();
	%this.loadFile.delete();

	//Align the build to the brick grid
	%this.updateSize();

	%pos = vectorAdd(%this.rootPosition, %this.rootToCenter);

	%shiftX = mCeil(getWord(%pos, 0) * 2 - %this.brickSizeX % 2) / 2 + (%this.brickSizeX % 2) / 4  - getWord(%pos, 0);
	%shiftY = mCeil(getWord(%pos, 1) * 2 - %this.brickSizeY % 2) / 2 + (%this.brickSizeY % 2) / 4  - getWord(%pos, 1);
	%shiftZ = mCeil(getWord(%pos, 2) * 5 - %this.brickSizeZ % 2) / 5 + (%this.brickSizeZ % 2) / 10 - getWord(%pos, 2);

	%this.rootPosition = vectorAdd(%shiftX SPC %shiftY SPC %shiftZ, %this.rootPosition);

	%this.minX = %this.minX + %shiftX;
	%this.maxX = %this.maxX + %shiftX;
	%this.minY = %this.minY + %shiftY;
	%this.maxY = %this.maxY + %shiftY;
	%this.minZ = %this.minZ + %shiftZ;
	%this.maxZ = %this.maxZ + %shiftZ;

	%this.updateSize();
	%this.updateHighlightBox();

	//Message client
	// %s1 = %this.brickCount == 1 ? "" : "s";
	// %s2 = %this.connectionCount == 1 ? "" : "s";

	// messageClient(%this.client, 'MsgProcessComplete', "\c6Finished loading selection, got \c3"
	// 	@ %this.brickCount @ "\c6 Brick" @ %s1 @ " with \c3" @ %this.connectionCount @ "\c6 Connection" @ %s2 @ "!");

	// %this.client.ndLastLoadTime = $Sim::Time;
	// %this.client.ndSetMode(NDM_PlantCopy);
}

//Set the size variables after selecting bricks
function TurnbasedLoader::updateSize(%this)
{
	%this.minSize = vectorSub(%this.minX SPC %this.minY SPC %this.minZ, %this.rootPosition);
	%this.maxSize = vectorSub(%this.maxX SPC %this.maxY SPC %this.maxZ, %this.rootPosition);

	%this.brickSizeX = mFloatLength((%this.maxX - %this.minX) * 2, 0);
	%this.brickSizeY = mFloatLength((%this.maxY - %this.minY) * 2, 0);
	%this.brickSizeZ = mFloatLength((%this.maxZ - %this.minZ) * 5, 0);

	%this.rootToCenter = vectorAdd(%this.minSize, vectorScale(vectorSub(%this.maxSize, %this.minSize), 0.5));
}

//Create or update the highlight box
function TurnbasedLoader::updateHighlightBox(%this)
{
	if(!isObject(%this.highlightBox))
		%this.highlightBox = ND_HighlightBox();

	if(!isObject(%this.ghostGroup))
	{
		%min = vectorAdd(%this.rootPosition, %this.minSize);
		%max = vectorAdd(%this.rootPosition, %this.maxSize);
		%this.highlightBox.setSize(%min, %max);
	}
	else
		%this.highlightBox.setSize(%this.getGhostWorldBox());
}

function TurnbasedLoader::startPlant(%this, %position, %angleID, %forcePlant)
{
	%this.forcePlant = %forcePlant;

	%this.plantSearchIndex = 0;
	%this.plantQueueIndex = 0;
	%this.plantQueueCount = 0;

	%this.plantSuccessCount = 0;
	%this.plantTrustFailCount = 0;
	%this.plantBlockedFailCount = 0;
	%this.plantMissingFailCount = 0;

	//Reset mirror error list
	%client = %this.client;

	%countX = $NS[%client, "MXC"];
	%countZ = $NS[%client, "MZC"];

	for(%i = 0; %i < %countX; %i++)
		$NS[%client, "MXK", $NS[%client, "MXE", %i]] = "";

	for(%i = 0; %i < %countZ; %i++)
		$NS[%client, "MZK", $NS[%client, "MZE", %i]] = "";

	$NS[%client, "MZC"] = 0;
	$NS[%client, "MXC"] = 0;

	//Make list of spawned clients to scope bricks
	%this.numClients = 0;

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);

		if(%cl.hasSpawnedOnce
		&& isObject(%obj = %cl.getControlObject())
		&& vectorDist(%this.ghostPosition, %obj.getTransform()) < 10000)
		{
			$NS[%this, "CL", %this.numClients] = %cl;
			%this.numClients++;
		}
	}

	if($Pref::Server::ND::PlayMenuSounds && %this.brickCount > $Pref::Server::ND::ProcessPerTick * 10)
		messageClient(%this.client, 'MsgUploadStart', "");
    talk("1");
	%this.tickPlantSearch($Pref::Server::ND::ProcessPerTick, %position, %angleID);
}

//Go through the list of bricks until we find one that plants successfully
function TurnbasedLoader::tickPlantSearch(%this, %remainingPlants, %position, %angleID)
{
	%start = %this.plantSearchIndex;
	%end = %start + %remainingPlants;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%client = %this.client;

	// if(isObject(%this.targetGroup))
	// {
	// 	%group = %this.targetGroup;
	// 	%bl_id = %this.targetBlid;
	// }
	// else
	// {
	// 	%group = %client.brickGroup.getId();
	// 	%bl_id = %client.bl_id;
	// }

	%qCount = %this.plantQueueCount;
	%numClients = %this.numClients;
	for(%i = %start; %i < %end; %i++)
	{
        talk("egg");
		//Brick already placed
		if($NP[%this, %i])
			continue;
            talk("egg");
		//Skip nonexistant bricks
		if($NS[%this, "D", %i] == 0)
		{
			$NP[%this, %i] = true;
			%this.plantMissingFailCount++;
			continue;
		}
        talk("egg");
		//Attempt to place brick
		%brick = TurnbasedLoader::plantBrick(%this, %i, %position, %angleID, %group, %client, %bl_id);
		%plants++;

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;
			//%this.undoGroup.add(%brick);

			$NP[%this, %i] = true;

			%conns = $NS[%this, "N", %i];
			for(%j = 0; %j < %conns; %j++)
			{
				%id = $NS[%this, "C", %i, %j];

				if(%id < %i && !$NP[%this, %id])
				{
					%found = true;

					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}

			//Instantly ghost the brick to all spawned clients (wow hacks)
			for(%j = 0; %j < %numClients; %j++)
			{
				%cl = $NS[%this, "CL", %j];
				%brick.scopeToClient(%cl);
				%brick.clearScopeToClient(%cl);
			}

			//If we added bricks to plant queue, switch to second loop
			if(%found)
			{
				%this.plantSearchIndex = %i + 1;
				%this.plantQueueCount = %qCount;
				%this.tickPlantTree(%remainingPlants - %plants, %position, %angleID);
				return;
			}

			%lastPos = %brick.position;
		}
		else if(%brick == -1)
		{
			$NP[%this, %i] = true;
			%this.plantBlockedFailCount++;
		}
		else if(%brick == -2)
		{
			$NP[%this, %i] = true;
			%this.plantTrustFailCount++;
		}
	}

	%this.plantSearchIndex = %i;
	%this.plantQueueCount = %qCount;

	if(strLen(%lastPos))
		serverPlay3D(BrickPlantSound, %lastPos);

	//Tell the client how far we got
	// if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	// {
	// 	%this.client.ndUpdateBottomPrint();
	// 	%this.client.ndLastMessageTime = $Sim::Time;
	// }
    talk("2");
	if(%end < %this.brickCount && %this.plantSuccessCount < %this.brickCount)
		%this.plantSchedule = %this.schedule(30, tickPlantSearch, $Pref::Server::ND::ProcessPerTick, %position, %angleID);
	else
		%this.finishPlant();
}

//Plant search has prepared a queue, plant all bricks in this queue and add their connected bricks aswell
function TurnbasedLoader::tickPlantTree(%this, %remainingPlants, %position, %angleID)
{
	%start = %this.plantQueueIndex;
	%end = %start + %remainingPlants;

	%client = %this.client;

	if(isObject(%this.targetGroup))
	{
		%group = %this.targetGroup;
		%bl_id = %this.targetBlid;
	}
	else
	{
		%group = %client.brickGroup.getId();
		%bl_id = %client.bl_id;
	}

	%qCount = %this.plantQueueCount;
	%numClients = %this.numClients;

	%searchIndex = %this.plantSearchIndex;

	for(%i = %start; %i < %end; %i++)
	{
		//The queue is empty! Switch back to plant search.
		if(%i >= %qCount)
		{
			if(strLen(%lastPos))
				serverPlay3D(BrickPlantSound, %lastPos);

			%this.plantQueueCount = %qCount;
			%this.plantQueueIndex = %i;
			%this.tickPlantSearch(%end - %i, %position, %angleID);
			return;
		}

		//Attempt to plant queued brick
		%bId = $NS[%this, "PQueue", %i];

		//Skip nonexistant bricks
		if($NS[%this, "D", %i] == 0)
		{
			$NP[%this, %bId] = true;
			%this.plantMissingFailCount++;
			continue;
		}

		%brick = ND_Selection::plantBrick(%this, %bId, %position, %angleID, %group, %client, %bl_id);

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;

			$NP[%this, %bId] = true;

			%conns = $NS[%this, "N", %bId];
			for(%j = 0; %j < %conns; %j++)
			{
				%id = $NS[%this, "C", %bId, %j];

				if(%id < %searchIndex && !$NP[%this, %id])
				{
					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}

			%lastPos = %brick.position;

			//Instantly ghost the brick to all spawned clients (wow hacks)
			for(%j = 0; %j < %numClients; %j++)
			{
				%cl = $NS[%this, "CL", %j];
				%brick.scopeToClient(%cl);
				%brick.clearScopeToClient(%cl);
			}
		}
		else if(%brick == -1)
		{
			%this.plantBlockedFailCount++;
			$NP[%this, %bId] = true;
		}
		else if(%brick == -2)
		{
			%this.plantTrustFailCount++;
			$NP[%this, %bId] = true;
		}
	}

	if(strLen(%lastPos))
		serverPlay3D(BrickPlantSound, %lastPos);

	//Tell the client how far we got
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	%this.plantQueueCount = %qCount;
	%this.plantQueueIndex = %i;
talk("3");
	%this.plantSchedule = %this.schedule(30, tickPlantTree, $Pref::Server::ND::ProcessPerTick, %position, %angleID);
}

//Attempt to plant brick with id %i
//Returns brick if planted, 0 if floating, -1 if blocked, -2 if trust failure
function TurnbasedLoader::plantBrick(%this, %i, %position, %angleID, %brickGroup, %client, %bl_id)
{
    talk("plants");
	//Offset position
	%bPos = $NS[%this, "P", %i];

	//Local angle id
	%bAngle = $NS[%this, "R", %i];

	//Apply mirror effects (ugh)
	%datablock = $NS[%this, "D", %i];

	// %mirrX = %this.ghostMirrorX;
	// %mirrY = %this.ghostMirrorY;
	// %mirrZ = %this.ghostMirrorZ;

	// if(%mirrX)
	// {
	// 	//Mirror offset
	// 	%bPos = -firstWord(%bPos) SPC restWords(%bPos);

	// 	//Handle symmetries
	// 	switch($ND::Symmetry[%datablock])
	// 	{
	// 		//Asymmetric
	// 		case 0:
	// 			if(%db = $ND::SymmetryXDatablock[%datablock])
	// 			{
	// 				%datablock = %db;
	// 				%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

	// 				//Pair is made on X, so apply mirror logic for X afterwards
	// 				if(%bAngle % 2 == 1)
	// 					%bAngle = (%bAngle + 2) % 4;
	// 			}
	// 			else
	// 			{
	// 				//Add datablock to list of mirror problems
	// 				if(!$NS[%client, "MXK", %datablock])
	// 				{
	// 					%id = $NS[%client, "MXC"];
	// 					$NS[%client, "MXC"]++;

	// 					$NS[%client, "MXE", %id] = %datablock;
	// 					$NS[%client, "MXK", %datablock] = true;
	// 				}
	// 			}

	// 		//Do nothing for fully symmetric

	// 		//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
	// 		case 2:
	// 			if(%bAngle % 2 == 1)
	// 				%bAngle = (%bAngle + 2) % 4;

	// 		//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
	// 		case 3:
	// 			if(%bAngle % 2 == 0)
	// 				%bAngle = (%bAngle + 2) % 4;

	// 		//X+Y symmetric - rotate 90 degrees
	// 		case 4:
	// 			if(%bAngle % 2 == 0)
	// 				%bAngle = (%bAngle + 1) % 4;
	// 			else
	// 				%bAngle = (%bAngle + 3) % 4;

	// 		//X-Y symmetric - rotate -90 degrees
	// 		case 5:
	// 			if(%bAngle % 2 == 0)
	// 				%bAngle = (%bAngle + 3) % 4;
	// 			else
	// 				%bAngle = (%bAngle + 1) % 4;
	// 	}
	// }
	// else if(%mirrY)
	// {
	// 	//Mirror offset
	// 	%bPos = getWord(%bPos, 0) SPC -getWord(%bPos, 1) SPC getWord(%bPos, 2);

	// 	//Handle symmetries
	// 	switch($ND::Symmetry[%datablock])
	// 	{
	// 		//Asymmetric
	// 		case 0:
	// 			if(%db = $ND::SymmetryXDatablock[%datablock])
	// 			{
	// 				%datablock = %db;
	// 				%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

	// 				//Pair is made on X, so apply mirror logic for X afterwards
	// 				if(%bAngle % 2 == 0)
	// 					%bAngle = (%bAngle + 2) % 4;
	// 			}
	// 			else
	// 			{
	// 				//Add datablock to list of mirror problems
	// 				if(!$NS[%client, "MXK", %datablock])
	// 				{
	// 					%id = $NS[%client, "MXC"];
	// 					$NS[%client, "MXC"]++;

	// 					$NS[%client, "MXE", %id]= %datablock;
	// 					$NS[%client, "MXK", %datablock] = true;
	// 				}
	// 			}

	// 		//Do nothing for fully symmetric

	// 		//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
	// 		case 2:
	// 			if(%bAngle % 2 == 0)
	// 				%bAngle = (%bAngle + 2) % 4;

	// 		//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
	// 		case 3:
	// 			if(%bAngle % 2 == 1)
	// 				%bAngle = (%bAngle + 2) % 4;

	// 		//X+Y symmetric - rotate 90 degrees
	// 		case 4:
	// 			if(%bAngle % 2 == 1)
	// 				%bAngle = (%bAngle + 1) % 4;
	// 			else
	// 				%bAngle = (%bAngle + 3) % 4;

	// 		//X-Y symmetric - rotate -90 degrees
	// 		case 5:
	// 			if(%bAngle % 2 == 1)
	// 				%bAngle = (%bAngle + 3) % 4;
	// 			else
	// 				%bAngle = (%bAngle + 1) % 4;
	// 	}
	// }

	// if(%mirrZ)
	// {
	// 	//Mirror offset
	// 	%bPos = getWords(%bPos, 0, 1) SPC -getWord(%bPos, 2);

	// 	//Change datablock if asymmetric
	// 	if(!$ND::SymmetryZ[%datablock])
	// 	{
	// 		if(%db = $ND::SymmetryZDatablock[%datablock])
	// 		{
	// 			%datablock = %db;
	// 			%bAngle = (%bAngle + $ND::SymmetryZOffset[%datablock]) % 4;
	// 		}
	// 		else
	// 		{
	// 			//Add datablock to list of mirror problems
	// 			if(!$NS[%client, "MZK", %datablock])
	// 			{
	// 				%id = $NS[%client, "MZC"];
	// 				$NS[%client, "MZC"]++;

	// 				$NS[%client, "MZE", %id]= %datablock;
	// 				$NS[%client, "MZK", %datablock] = true;
	// 			}
	// 		}
	// 	}
	// }

	//Rotate and add offset
	%bAngle = (%bAngle + %angleID) % 4;
	%bPos = vectorAdd(%position, ndRotateVector(%bPos, %angleID));

	switch(%bAngle)
	{
		case 0: %bRot = "1 0 0 0";
		case 1: %bRot = "0 0 1 90.0002";
		case 2: %bRot = "0 0 1 180";
		case 3: %bRot = "0 0 -1 90.0002";
	}

	//Attempt to plant brick
	%brick = new FxDTSBrick()
	{
		datablock = %datablock;
		isPlanted = true;
		client = %client;

		position = %bPos;
		rotation = %bRot;
		angleID = %bAngle;

		colorID = $NS[%this, "CO", %i];
		colorFxID = $NS[%this, "CF", %i];

		printID = $NS[%this, "PR", %i];
	};

	//This will call ::onLoadPlant instead of ::onPlant
	%prev1 = $Server_LoadFileObj;
	%prev2 = $LastLoadedBrick;
	$Server_LoadFileObj = %brick;
	$LastLoadedBrick = %brick;
    talk(%brick);
	//Add to brickgroup
	mainBrickGroup.getObject(1).add(%brick);
    %brick.setTrusted(1);
    %brick.plant();

    $LoadToTurnbased.add(%brick);
	//Attempt plant
	// %error = %brick.plant();

	//Restore variable
	$Server_LoadFileObj = %prev1;
	$LastLoadedBrick = %prev2;

	// if(!isObject(%brick))
	// 	return -1;

	// if(%error == 2)
	// {
	// 	//Do we plant floating bricks?
	// 	if(%this.forcePlant)
	// 	{
	// 		//Brick is floating. Pretend it is supported by terrain
	// 		%brick.isBaseplate = true;

	// 		//Make engine recompute distance from ground to apply it
	// 		%brick.willCauseChainKill();
	// 	}
	// 	else
	// 	{
	// 		%brick.delete();
	// 		return 0;
	// 	}
	// }
	// else if(%error)
	// {
	// 	%brick.delete();
	// 	return -1;
	// }

	//Check for trust
	// %downCount = %brick.getNumDownBricks();

	// if(!%client.isAdmin || !$Pref::Server::ND::AdminTrustBypass2)
	// {
	// 	for(%j = 0; %j < %downCount; %j++)
	// 	{
	// 		if(!ndFastTrustCheck(%brick.getDownBrick(%j), %bl_id, %brickGroup))
	// 		{
	// 			%brick.delete();
	// 			return -2;
	// 		}
	// 	}

	// 	%upCount = %brick.getNumUpBricks();

	// 	for(%j = 0; %j < %upCount; %j++)
	// 	{
	// 		if(!ndFastTrustCheck(%brick.getUpBrick(%j), %bl_id, %brickGroup))
	// 		{
	// 			%brick.delete();
	// 			return -2;
	// 		}
	// 	}
	// }
	// else if(!%downCount)
	// 	%upCount = %brick.getNumUpBricks();

	// //Finished trust check
	// if(%downCount)
	// 	%brick.stackBL_ID = %brick.getDownBrick(0).stackBL_ID;
	// else if(%upCount)
	// 	%brick.stackBL_ID = %brick.getUpBrick(0).stackBL_ID;
	// else
	// 	%brick.stackBL_ID = %bl_id;

	// %brick.trustCheckFinished();

	//Apply special settings
	%brick.setRendering(!$NS[%this, "NR", %i]);
	%brick.setRaycasting(!$NS[%this, "NRC", %i]);
	%brick.setColliding(!$NS[%this, "NC", %i]);
	%brick.setShapeFx($NS[%this, "SF", %i]);

	//Apply events
	// if(%numEvents = $NS[%this, "EN", %i])
	// {
	// 	%brick.numEvents = %numEvents;
	// 	%brick.implicitCancelEvents = 0;

	// 	for(%j = 0; %j < %numEvents; %j++)
	// 	{
	// 		%brick.eventEnabled[%j] = $NS[%this, "EE", %i, %j];
	// 		%brick.eventDelay[%j] = $NS[%this, "ED", %i, %j];

	// 		%inputIdx = $NS[%this, "EII", %i, %j];

	// 		%brick.eventInput[%j] = $NS[%this, "EI", %i, %j];
	// 		%brick.eventInputIdx[%j] = %inputIdx;

	// 		%target = $NS[%this, "ET", %i, %j];
	// 		%targetIdx = $NS[%this, "ETI", %i, %j];

	// 		if(%targetIdx == -1)
	// 		{
	// 			%nt = $NS[%this, "ENT", %i, %j];
	// 			%brick.eventNT[%j] = %nt;
	// 		}

	// 		%brick.eventTarget[%j] = %target;
	// 		%brick.eventTargetIdx[%j] = %targetIdx;

	// 		%output = $NS[%this, "EO", %i, %j];
	// 		%outputIdx = $NS[%this, "EOI", %i, %j];

	// 		//Only rotate outputs for named bricks if they are selected
	// 		if(%targetIdx >= 0 || $NS[%this, "HN", %nt])
	// 		{
	// 			//Rotate fireRelay events
	// 			switch$(%output)
	// 			{
	// 				case "fireRelayUp":    %dir = 0;
	// 				case "fireRelayDown":  %dir = 1;
	// 				case "fireRelayNorth": %dir = 2;
	// 				case "fireRelayEast":  %dir = 3;
	// 				case "fireRelaySouth": %dir = 4;
	// 				case "fireRelayWest":  %dir = 5;
	// 				default: %dir = -1;
	// 			}

	// 			if(%dir >= 0)
	// 			{
	// 				%rotated = ndTransformDirection(%dir, %angleID, %mirrX, %mirrY, %mirrZ);
	// 				%outputIdx += %rotated - %dir;

	// 				switch(%rotated)
	// 				{
	// 					case 0: %output = "fireRelayUp";
	// 					case 1: %output = "fireRelayDown";
	// 					case 2: %output = "fireRelayNorth";
	// 					case 3: %output = "fireRelayEast";
	// 					case 4: %output = "fireRelaySouth";
	// 					case 5: %output = "fireRelayWest";
	// 				}
	// 			}
	// 		}

	// 		%brick.eventOutput[%j] = %output;
	// 		%brick.eventOutputIdx[%j] = %outputIdx;
	// 		%brick.eventOutputAppendClient[%j] = $NS[%this, "EOC", %i, %j];

	// 		//Why does this need to be so complicated?
	// 		if(%targetIdx >= 0)
	// 			%targetClass = getWord($InputEvent_TargetListfxDtsBrick_[%inputIdx], %targetIdx * 2 + 1);
	// 		else
	// 			%targetClass = "FxDTSBrick";

	// 		%paramList = $OutputEvent_ParameterList[%targetClass, %outputIdx];
	// 		%paramCount = getFieldCount(%paramList);

	// 		for(%k = 0; %k < %paramCount; %k++)
	// 		{
	// 			%param = $NS[%this, "EP", %i, %j, %k];

	// 			//Only rotate outputs for named bricks if they are selected
	// 			if(%targetIdx >= 0 || $NS[%this, "HN", %nt])
	// 			{
	// 				%paramType = getField(%paramList, %k);

	// 				switch$(getWord(%paramType, 0))
	// 				{
	// 					case "vector":
	// 						//Apply mirror effects
	// 						if(%mirrX)
	// 							%param = -firstWord(%param) SPC restWords(%param);
	// 						else if(%mirrY)
	// 							%param = getWord(%param, 0) SPC -getWord(%param, 1) SPC getWord(%param, 2);

	// 						if(%mirrZ)
	// 							%param = getWord(%param, 0) SPC getWord(%param, 1) SPC -getWord(%param, 2);

	// 						%param = ndRotateVector(%param, %angleID);

	// 					case "list":
	// 						%value = getWord(%paramType, %param * 2 + 1);

	// 						switch$(%value)
	// 						{
	// 							case "Up":    %dir = 0;
	// 							case "Down":  %dir = 1;
	// 							case "North": %dir = 2;
	// 							case "East":  %dir = 3;
	// 							case "South": %dir = 4;
	// 							case "West":  %dir = 5;
	// 							default: %dir = -1;
	// 						}

	// 						if(%dir >= 0)
	// 						{
	// 							switch(ndTransformDirection(%dir, %angleID, %mirrX, %mirrY, %mirrZ))
	// 							{
	// 								case 0: %value = "Up";
	// 								case 1: %value = "Down";
	// 								case 2: %value = "North";
	// 								case 3: %value = "East";
	// 								case 4: %value = "South";
	// 								case 5: %value = "West";
	// 							}

	// 							for(%l = 1; %l < getWordCount(%paramType); %l += 2)
	// 							{
	// 								if(getWord(%paramType, %l) $= %value)
	// 								{
	// 									%param = getWord(%paramType, %l + 1);
	// 									break;
	// 								}
	// 							}
	// 						}
	// 				}
	// 			}

	// 			%brick.eventOutputParameter[%j, %k + 1] = %param;
	// 		}
	// 	}
	// }

	setCurrentQuotaObject(getQuotaObjectFromClient(%client));

	if((%tmp = $NS[%this, "NT", %i]) !$= "")
		%brick.setNTObjectName(%tmp);

	if(%tmp = $NS[%this, "LD", %i])
		%brick.setLight(%tmp, %client);

	if(%tmp = $NS[%this, "ED", %i])
	{
		%dir = ndTransformDirection($NS[%this, "ER", %i], %angleID, %mirrX, %mirrY, %mirrZ);

		%brick.emitterDirection = %dir;
		%brick.setEmitter(%tmp, %client);
	}

	if(%tmp = $NS[%this, "ID", %i])
	{
		%pos = ndTransformDirection($NS[%this, "IP", %i], %angleID, %mirrX, %mirrY, %mirrZ);
		%dir = ndTransformDirection($NS[%this, "IR", %i], %angleID, %mirrX, %mirrY, %mirrZ);

		%brick.itemPosition = %pos;
		%brick.itemDirection = %dir;
		%brick.itemRespawnTime = $NS[%this, "IT", %i];
		%brick.setItem(%tmp, %client);
	}

	if(%tmp = $NS[%this, "VD", %i])
	{
		%brick.reColorVehicle = $NS[%this, "VC", %i];
		%brick.setVehicle(%tmp, %client);
	}

	if(%tmp = $NS[%this, "MD", %i])
		%brick.setSound(%tmp, %client);

	return %brick;
}

//Finished planting all the bricks!
function TurnbasedLoader::finishPlant(%this)
{
    talk("Done");
}

function ndCreateByte241Table()
{
	$ND::Byte241Lookup = "";

	//This will map uints 0-241 to chars 15-255, starting after \r
	for(%i = 15; %i < 256; %i++)
	{
		%char = collapseEscape("\\x" @
			getSubStr("0123456789abcdef", (%i & 0xf0) >> 4, 1) @
			getSubStr("0123456789abcdef", %i & 0x0f, 1));

		$ND::Byte241ToChar[%i - 15] = %char;
		$ND::Byte241Lookup = $ND::Byte241Lookup @ %char;
	}

	$ND::Byte241TableCreated = true;
}

//Packs uint in single byte
function ndPack241_1(%num)
{
	return $ND::Byte241ToChar[%num];
}

//Packs uint in two bytes
function ndPack241_2(%num)
{
	return $ND::Byte241ToChar[(%num / 241) | 0] @ $ND::Byte241ToChar[%num % 241];
}

//Packs uint in three bytes
function ndPack241_3(%num)
{
	return
		$ND::Byte241ToChar[(((%num / 241) | 0) / 241) | 0] @
		$ND::Byte241ToChar[((%num / 241) | 0) % 241] @
		$ND::Byte241ToChar[%num % 241];
}

//Packs uint in four bytes
function ndPack241_4(%num)
{
	return
		$ND::Byte241ToChar[(((((%num / 241) | 0) / 241) | 0) / 241) | 0] @
		$ND::Byte241ToChar[((((%num / 241) | 0) / 241) | 0) % 241] @
		$ND::Byte241ToChar[((%num / 241) | 0) % 241] @
		$ND::Byte241ToChar[%num % 241];
}

//Unpacks uint from single byte
function ndUnpack241_1(%subStr)
{
	return strStr($ND::Byte241Lookup, %subStr);
}

//Unpacks uint from two bytes
function ndUnpack241_2(%subStr)
{
	return
		strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 241 +
		strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1));
}

//Unpacks uint from three bytes
function ndUnpack241_3(%subStr)
{
	return
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 58081) | 0) +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1)) *   241       +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 2, 1));
}

//Unpacks uint from four bytes
function ndUnpack241_4(%subStr)
{
	return
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 13997521) | 0) +
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1)) *    58081) | 0) +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 2, 1)) *      241       +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 3, 1));
}



//Binary compression (command version, 255 allowed characters)
///////////////////////////////////////////////////////////////////////////

//Creates byte lookup table
function ndCreateByte255Table()
{
	$ND::Byte255Lookup = "";

	//This will map uints 0-254 to chars 1-255, starting after \x00
    for(%i = 1; %i < 256; %i++)
    {
        %char = collapseEscape("\\x" @
          getSubStr("0123456789abcdef", (%i & 0xf0) >> 4, 1) @
          getSubStr("0123456789abcdef", %i & 0x0f, 1));

        $ND::Byte255ToChar[%i - 1] = %char;
        $ND::Byte255Lookup = $ND::Byte255Lookup @ %char;
    }

	$ND::Byte255TableCreated = true;
}

//Packs uint in single byte
function ndPack255_1(%num)
{
	return $ND::Byte255ToChar[%num];
}

//Packs uint in two bytes
function ndPack255_2(%num)
{
	return $ND::Byte255ToChar[(%num / 255) | 0] @ $ND::Byte255ToChar[%num % 255];
}

//Packs uint in three bytes
function ndPack255_3(%num)
{
	return
		$ND::Byte255ToChar[(((%num / 255) | 0) / 255) | 0] @
		$ND::Byte255ToChar[((%num / 255) | 0) % 255] @
		$ND::Byte255ToChar[%num % 255];
}

//Packs uint in four bytes
function ndPack255_4(%num)
{
	return
		$ND::Byte255ToChar[(((((%num / 255) | 0) / 255) | 0) / 255) | 0] @
		$ND::Byte255ToChar[((((%num / 255) | 0) / 255) | 0) % 255] @
		$ND::Byte255ToChar[((%num / 255) | 0) % 255] @
		$ND::Byte255ToChar[%num % 255];
}

//Unpacks uint from single byte
function ndUnpack255_1(%subStr)
{
	return strStr($ND::Byte255Lookup, %subStr);
}

//Unpacks uint from two bytes
function ndUnpack255_2(%subStr)
{
	return
		strStr($ND::Byte255Lookup, getSubStr(%subStr, 0, 1)) * 255 +
		strStr($ND::Byte255Lookup, getSubStr(%subStr, 1, 1));
}

//Unpacks uint from three bytes
function ndUnpack255_3(%subStr)
{
	return
		((strStr($ND::Byte255Lookup, getSubStr(%subStr, 0, 1)) * 65025) | 0) +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 1, 1)) *   255       +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 2, 1)) | 0;
}

//Unpacks uint from four bytes
function ndUnpack255_4(%subStr)
{
	return
		((strStr($ND::Byte255Lookup, getSubStr(%subStr, 0, 1)) * 16581375) | 0) +
		((strStr($ND::Byte255Lookup, getSubStr(%subStr, 1, 1)) *    65025) | 0) +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 2, 1)) *      255       +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 3, 1)) | 0;
}

//Some tests for the packing functions
function ndTestPack255()
{
	echo("Testing 1 byte");
	echo(ndUnpack255_1(ndPack255_1(0)) == 0);
	echo(ndUnpack255_1(ndPack255_1(123)) == 123);
	echo(ndUnpack255_1(ndPack255_1(231)) == 231);
	echo(ndUnpack255_1(ndPack255_1(254)) == 254);

	echo("Testing 2 byte");
	echo(ndUnpack255_2(ndPack255_2(0)) == 0);
	echo(ndUnpack255_2(ndPack255_2(123)) == 123);
	echo(ndUnpack255_2(ndPack255_2(231)) == 231);
	echo(ndUnpack255_2(ndPack255_2(254)) == 254);
	echo(ndUnpack255_2(ndPack255_2(12345)) == 12345);
	echo(ndUnpack255_2(ndPack255_2(32145)) == 32145);
	echo(ndUnpack255_2(ndPack255_2(65024)) == 65024);

	echo("Testing 3 byte");
	echo(ndUnpack255_3(ndPack255_3(0)) == 0);
	echo(ndUnpack255_3(ndPack255_3(123)) == 123);
	echo(ndUnpack255_3(ndPack255_3(231)) == 231);
	echo(ndUnpack255_3(ndPack255_3(254)) == 254);
	echo(ndUnpack255_3(ndPack255_3(12345)) == 12345);
	echo(ndUnpack255_3(ndPack255_3(32145)) == 32145);
	echo(ndUnpack255_3(ndPack255_3(65024)) == 65024);
	echo(ndUnpack255_3(ndPack255_3(11234567)) == 11234567);
	echo(ndUnpack255_3(ndPack255_3(14132451)) == 14132451);
	echo(ndUnpack255_3(ndPack255_3(16581374)) == 16581374);

	echo("Testing 4 byte");
	echo(ndUnpack255_4(ndPack255_4(0)) == 0);
	echo(ndUnpack255_4(ndPack255_4(123)) == 123);
	echo(ndUnpack255_4(ndPack255_4(231)) == 231);
	echo(ndUnpack255_4(ndPack255_4(254)) == 254);
	echo(ndUnpack255_4(ndPack255_4(12345)) == 12345);
	echo(ndUnpack255_4(ndPack255_4(32145)) == 32145);
	echo(ndUnpack255_4(ndPack255_4(65024)) == 65024);
	echo(ndUnpack255_4(ndPack255_4(11234567)) == 11234567);
	echo(ndUnpack255_4(ndPack255_4(14132451)) == 14132451);
	echo(ndUnpack255_4(ndPack255_4(16581374)) == 16581374);
	echo(ndUnpack255_4(ndPack255_4(1234567890)) == 1234567890);

	//Appearantly tork uses uint and normal int randomly in
	//seperate places so we can't use the full uint range
	echo(ndUnpack255_4(ndPack255_4(2147483647)) == 2147483647);
	echo(ndUnpack255_4(ndPack255_4(2147483648)) != 2147483648);
}