(()=>{"use strict";class e{applyInjectedCss(e){let t=document.createElement("style");t.type="text/css",t.innerHTML=e,document.head.appendChild(t)}populatePlayerList(e,t){t.logData("Starting to populate selection menu");const n=t.HTMLNodes.playerSelectList;n.innerHTML="",e.length>0?(t.logData("Starting to populate selection menu"),e.forEach((e=>{t.logData(`Adding option for player: ${e.displayName}, ID: ${e.playerId}`);let o=document.createElement("li");o.classList.add("dropdown_item"),o.innerText=e.displayName,o.dataset.playerId=e.playerId,o.addEventListener("click",(()=>{t.HTMLNodes.playerSelectDropdown.innerText=e.displayName,t.HTMLNodes.selectedPlayerTitle.innerText="Interacting with player: "+e.displayName,n.classList.add("hide"),t.selectedPlayer=e.playerId,t.logData(t.selectedPlayer),t.logData(`Selected player: ${e.displayName} (ID: ${e.playerId})`)})),n.appendChild(o)})),n.classList.toggle("hide"),t.logData(t.HTMLNodes.playerSelectList.innerHTML)):t.logData("No players found in the list.")}outputCSSRules(e,t){}dumpScriptContentByName(e,t,n){let o="";n._logToDebugPanel(`Dumping content for script: ${t}\n`);const i=document.querySelectorAll("script");n._logToDebugPanel(`Number of script tags found: ${i.length}\n`);const s=Array.from(i).find((e=>e.src===t));if(s)n._logToDebugPanel(`Target script found: ${s.src}\n`),fetch(s.src).then((e=>(e.ok||n._logToDebugPanel(`Network response was not ok: ${e.statusText}`),e.text()))).then((e=>{o+=`/* ${s.src} */\n\n${e}\n\n`,n._logToDebugPanel("Content fetched successfully.\n"),n._logToDebugPanel(o)})).catch((e=>{n._logToDebugPanel(`Error fetching script content: ${e.message}\n`)}));else{n._logToDebugPanel("Script not found or is an inline script.\n"),n._logToDebugPanel("Searching for inline scripts...\n");const e=Array.from(i).find((e=>e.textContent.includes(t)));e?(n._logToDebugPanel("Inline script found. Displaying content...\n"),o+=`/* Inline script */\n\n${e.textContent}\n\n`,n._logToDebugPanel(o)):n._logToDebugPanel("Inline script not found.\n")}}}class t{disownConstruct(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",2004,[],JSON.stringify({id:e}))}takeOverConstruct(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",2008,[],JSON.stringify({id:e}))}repairConstruct(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",2009,[],JSON.stringify({id:e}))}removeDRMproctection(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",2010,[],JSON.stringify({id:e}))}repairElement(e,t){!1!==e&&(this.logData(e+","+t),CPPMod.sendModAction("AkimboAdmin",2005,[],JSON.stringify({id:e,constructId:t})))}BypassDispenser(e,t){!1!==e&&CPPMod.sendModAction("AkimboAdmin",2006,[],JSON.stringify({id:e,constructId:t}))}ResetDispenser(e,t){!1!==e&&CPPMod.sendModAction("AkimboAdmin",2007,[],JSON.stringify({id:e,constructId:t}))}configureTeleporter(e,t,n,o){this.logData(e+" "+t+" "+n+" "+o),!1!==n&&CPPMod.sendModAction("AkimboAdmin",2012,[],JSON.stringify({id:e,constructId:t,teleportName:n,type:o}))}addTeleportDestination(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3001,[],JSON.stringify({teleportName:e}))}ClaimOwnedTerritory(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3002,[],JSON.stringify({tag:e}))}ClaimUnownedTerritory(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3003,[],JSON.stringify({tag:e}))}searchForPlayer(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3004,[],JSON.stringify({name:e}))}teleportToPlayer(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3005,[],JSON.stringify({id:e}))}teleportPlayerHere(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3006,[],JSON.stringify({id:e}))}teleportToCoordinates(e){!1!==e&&(akimboAdminHud.HTMLNodes.customPos.value="",CPPMod.sendModAction("AkimboAdmin",3007,[],JSON.stringify({pos:e})))}teleportPlayerToCoordinates(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3008,[],JSON.stringify({pos:e}))}sendTeleportLocation(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3009,[],JSON.stringify({tag:e}))}fillInventory(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3010,[],JSON.stringify({id:e}))}fillPlayerInventory(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3011,[],JSON.stringify({id:e}))}clearInventory(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3012,[],JSON.stringify({id:e}))}clearPlayerInventory(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",3013,[],JSON.stringify({id:e}))}AddItemToInventory(e,t,n){!1!==e&&!1!==t&&CPPMod.sendModAction("AkimboAdmin",3014,[],JSON.stringify({id:e,itemId:t,quantity:n}))}AddItemToPlayerInventory(e,t,n){!1!==e&&!1!==t&&CPPMod.sendModAction("AkimboAdmin",3015,[],JSON.stringify({id:e,itemId:t,quantity:n}))}logData(e){!1!==e&&CPPMod.sendModAction("AkimboAdmin",888,[],JSON.stringify({data:e}))}}class n extends MousePage{constructor(){super(),this.adminFunctions=new e,this.adminModInteraction=new t,this._createHTML(),this.logData=this.adminModInteraction.logData,this.wrapperNode.classList.add("hide"),engine.on("AkimboAdminConstructHud.show",this.showIt,this),engine.on("AkimboAdminConstructHud.setConstructInfo",this.setConstructInfo,this),"undefined"!=typeof injectedCss&&this.adminFunctions.applyInjectedCss(injectedCss)}setConstructInfo(e){var t=JSON.parse(e);this.HTMLNodes.ConstructName.innerText="Construct Name: "+t.Name,this.HTMLNodes.ConstructId.innerText="Construct Id: "+t.Id,this.HTMLNodes.OwnerName.innerText="Construct Owner: "+t.Owner.name,this.cInfo=t,this.logData(t),this.logData(this.cInfo.Id)}showIt(e){e?(hudManager.toggleEnhancedMouse(),this.show(!0)):this.show(!1)}show(e){super.show(e)}_onVisibilityChange(){super._onVisibilityChange(),this.wrapperNode.classList.toggle("hide",!this.isVisible),this.isVisible&&inputCaptureManager.captureText(!0,(()=>this._close())),this.isVisible||inputCaptureManager.captureText(!1)}_close(){this.show(!1),hudManager.toggleEnhancedMouse()}_createHTML(){this.HTMLNodes={},this.wrapperNode=createElement(document.body,"div",["AkimboAdminConstructHud_panel"]);let e=createElement(this.wrapperNode,"div",["AkimboAdminConstructHud_header"]);this.HTMLNodes.panelTitle=createElement(e,"h5",["panel_title"]),this.HTMLNodes.panelTitle.innerText="Akimbo Construct HUD",this.HTMLNodes.closeIconButton=createElement(e,"button",["closeConstructPanel_button"]),this.HTMLNodes.closeIconButton.innerText="Close",this.HTMLNodes.closeIconButton.addEventListener("click",(()=>this._close()));let t=createElement(this.wrapperNode,"div",["ConstructRow-flex-center"]);this.HTMLNodes.ConstructName=createElement(t,"h5",["construct-sub-title"]),this.HTMLNodes.ConstructId=createElement(t,"h5",["construct-sub-title"]),this.HTMLNodes.OwnerName=createElement(t,"h5",["construct-sub-title"]),this.HTMLNodes.ConstructName.innerText="Construct: <Name Come Here Now !>";let n=createElement(this.wrapperNode,"div",["row-flex-center"]),o=createElement(n,"button",["dump-button"]);o.innerText="Disown construct",o.addEventListener("click",(()=>{this.adminModInteraction.disownConstruct(this.cInfo.Id)}));let i=createElement(n,"button",["dump-button"]);i.innerText="Take over construct",i.addEventListener("click",(()=>{this.adminModInteraction.takeOverConstruct(this.cInfo.Id)}));let s=createElement(n,"button",["dump-button"]);s.innerText="Repair construct",s.addEventListener("click",(()=>{this.adminModInteraction.repairConstruct(this.cInfo.Id)}));let d=createElement(n,"button",["dump-button"]);d.innerText="Remove DRM protection",d.addEventListener("click",(()=>{this.adminModInteraction.removeDRMproctection(this.cInfo.Id)}))}}new n})();