# Friend Accepter Plugin for ArchiSteamFarm
ASF plugin for automatically accepting incoming friend requests.

## Installation
- Download the .zip file from the ![GitHub Release](https://img.shields.io/github/v/release/JackieWaltRyan/FriendAccepter?display_name=tag&logo=github&label=latest%20release&link=https%3A%2F%2Fgithub.com%2FJackieWaltRyan%2FFriendAccepter%2Freleases%2Flatest).
- Locate the `plugins` folder inside your ASF folder. Create a new folder here and unpack the downloaded .zip file to
  that folder.
- (Re)start ASF, you should get a message indicating that the plugin loaded successfully.

## Usage
### Enabling the plugin
You can enable the plugin per individual bot by adding `EnableFriendAccepter` to that bot's config file:

#### Your Bot.json config file:
```json
{
  "EnableFriendAccepter": true
}
```
