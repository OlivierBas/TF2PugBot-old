# TF2PugBot

Bot that simplifies the process of picking Team captains and Medics for teams. Bot is easy to configure once running.
It does not have a pick system ~~nor track amount of games played~~ but tracks all med immune players and allows for easy management of immunities.

This bot can be used in multiple servers, but will struggle running multiple pugs/mixes within one server.

Update (Release v0.1)
The bot now tracks games played for each player within every server as well as won spins. These stats can be seen with `/stats`. 

* [Commands](#commands)
  * [Basic](#basic)
  * [Management](#management)
  * [Administration](#administration)
* [Running](#running)
  * [No Compile](#release-no-compilation)     
  * [EasySetup.cs](#easysetupcs-compile)
* [Known Issues](#known-issues)

## Commands

#### Basic
* `/spinforcaptain`   = Spins for Team captain in the voice channel the user is currently in.
* `/spinformedic`     = Spins for Medic in the voice channel the current user is if the configuration has set that channel to a team voice channel.
* `/immunity get`     = Lists all Medic Immune players in the server
* `/stats {user?}`    = Lists the stats for the current user or the specified user if one is specified.

#### Management
* `/configure-admins {user}`                 = Sets the role allowed to configure the bot and use administrative commands.
* `/configure-channels {blu/red} {channel}`  = Sets the BLU/RED channel to the specified channel, used for medic spins.
* `/configure-pings {value}`                 = Enables or disables mention pings done by bot on a finished spin. 

#### Administration
* `/immunity grant {user}` = Grants the user 12 hour med immunity
* `/immunity revoke {user}` = Revokes the user's med immunity.


## Running

### Release (No Compilation)

The bot can be ran with the pre-compiled files found under [Releases](https://github.com/OlivierDotNet/TF2PugBot/releases), a bat or shell script file might be required to run the bot with necessary launch arguements. However these arguements can also be provided with shortcut launch arguements on Windows.

* Make a file called `run.sh` (Linx) / `run.bat` (Windows)
* Edit the file to execute the file with the following arguements:
  
  `./TF2PugBot {token} {guildId} {devId} {instantSpin}` (Linux)

  `"TF2PugBot.exe" {token} {guildId} {devId} {instantSpin}` (Windows)

* Run the file

**Parameters**:
* `token`       = The Discord Bot Token
* `guildID`     = The main Discord Server ID to be used. Purpose is for fast command iteration
* `devId`       = The User Id of the bot hoster, will grant the user full permissions for the bot in any server.
* `instantSpin` = Can be true or false, when false spins will be animated and slower while true will give instant results. *(Recommended to be **true** because of lack of multithreading which will affect bot performance)*

More options are available in [EasySetup.cs](https://github.com/OlivierDotNet/TF2PugBot/blob/main/EasySetup.cs) however will require compilation of the source code.

### EasySetup.cs (Compile)

* Open the solution in your favourite IDE
* Open EasySetup.cs and change values accordingly, every value is documented.
* Compile & Run application

## Known Issues

* `InstantSpins` option set to true will freeze the bot due to the lack of multithreading. Will also be very slow due to Discord rate limiting.
* The bot is made for single-pug servers in mind. Any attempt to run a secondary pug or mix will results in statistics being wrong.
  * This is also the case for med immunities.
