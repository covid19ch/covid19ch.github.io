DxBlazorInternal.define("cjs-chunk-dcd3551a.js",(function(e,t,n){var i=e("./cjs-dom-utils-c4472fdd.js"),o=function(e){this.eventTokens=[],this.context=e};o.prototype.attachEventWithContext=function(e,t,n,o){if(!this.eventTokens||!o||!e)return{item:null,name:null,delegate:null,Dispose:function(){}};var s=this,l=n.bind(o);i.attachEventToElement(e,t,l);var u={item:e,name:t,delegate:l,dispose:function(){s.detachEvent(this)}};return this.eventTokens.push(u),u},o.prototype.attachEvent=function(e,t,n){return this.attachEventWithContext(e,t,n,this.context)},o.prototype.detachEvent=function(e){if(!this.eventTokens)return null;if(!e||!e.item)return!1;var t=this.eventTokens.indexOf(e);return!!(t&&t>0)&&(i.detachEventFromElement(e.item,e.name,e.delegate),this.eventTokens.splice(t,1),e.item=null,e.delegate=null,e.Dispose=function(){},!0)},o.prototype.dispose=function(){return!!this.eventTokens&&(this.eventTokens.forEach((function(e){e.item&&(i.detachEventFromElement(e.item,e.name,e.delegate),e.item=null,e.delegate=null,e.Dispose=function(){})})),this.eventTokens=null,!0)},n.EventRegister=o}),["cjs-dom-utils-c4472fdd.js"]);