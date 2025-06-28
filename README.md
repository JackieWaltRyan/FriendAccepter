# Friend Accepter Plugin for ArchiSteamFarm

ASF plugin for automatically accepting incoming friend requests.

## Installation

- Download the .zip file from
  the [![GitHub Release](https://img.shields.io/github/v/release/JackieWaltRyan/FriendAccepter?display_name=tag&logo=github&label=latest%20release)](https://github.com/JackieWaltRyan/FriendAccepter/releases/latest)
- Locate the `plugins` folder inside your ASF folder. Create a new folder here and unpack the downloaded .zip file to
  that folder.
- (Re)start ASF, you should get a message indicating that the plugin loaded successfully.

## Usage

### Accept Friends Enable

Automatically accept all incoming friend requests. You can enable the plugin per individual bot by adding to that bot's
config file:

```json
{
  "AcceptFriendsEnable": true
}
```

### Group Auto Post Enable

For friend collectors only. The plugin can automatically post a comment to a group to find friends. To enable and
configure this feature, add the parameter to your bot's configuration file:

```json
{
  "GroupAutoPostEnable": true
}
```

#### Auto Post Configuration

To change this feature, add the following parameter to your bot's config file:

```json
{
  "GroupAutoPostConfig": {
    "GroupID": 103582791432987389,
    "Comment": "Feel free to add me! Accept everyone!",
    "Timeout": 60
  }
}
```

- `GroupID` - `ulong` type with default value of
  `103582791432987389` ([Search for Friends](https://steamcommunity.com/groups/SearchForFriends) group). ID of the group
  to which the comment will be sent.


- `Comment` - `string` type with default value of `Feel free to add me! Accept everyone!`. The text of the comment that
  will be published in the group.


- `Timeout` - `uint` type with default value of `60`. The interval in minutes after which the comment will be sent
  again.
