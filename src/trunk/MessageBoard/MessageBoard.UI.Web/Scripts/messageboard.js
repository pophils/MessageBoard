
var messageBoard = messageBoard || {};

(function ($, messageBoard) {
    messageBoard.domain = messageBoard.domain || {};

    var message = function (content) {
      
        return {
            content: content,
            
            datesaved: null,
            
            save: function (callback, view, self) {
                
                if (typeof content == "string" && content.length > 0) {
                    $.post("/save-message", { Content: this.content }, function (data) {
                        if (!data.Success) {
                            throw new Error("An error occured saving message, please try again."); 
                        } else {
                            window.refreshStartId = data.CurrentRefreshStartId;
                            
                            callback();
                            var messageArr = [];
                            messageArr.push(self);
                            console.log(self);
                            view.render(messageArr);
                        }
                        
                    });
                } else {
                    throw new Error("Message content can't be empty");
                }
            },
            
            loadTrendingMessages: function (messageView, updateElem) {
          
            $.getJSON("/get-trending-messages", { refreshStartId: window.refreshStartId }, function (data) {

                   if (data != null && data.Messages != null) {
                        for (var i = 0; i < data.Messages.length; i++) {
                            messageView.trendingMessStack.push(data.Messages[i]);
                        }
                        messageView.showUpdate(updateElem);
                        window.refreshStartId = data.CurrentRefreshStartId;
                    }
                });
            },
            
            loadPreviousMessages: function (messageView) {
              
                $.getJSON("/load-previous-messages", {
                    firstMessageIdOnFirstLoad: window.firstMessageIdOnFirstLoad, currentNumOfMessFetched: window.currentNumOfMessFetched,
                    pageNo: window.pageNo + 1}, function (data) {

                    var models = [];

                    if (data != null && data.Messages != null) {

                        for (var i = 0; i < data.Messages.length; i++) {
                            models.push(data.Messages[i]);
                        }
                        messageView.renderPrevious(models);
                        window.currentNumOfMessFetched = window.currentNumOfMessFetched + models.length;
                        window.pageNo = window.pageNo + 1;
                        
                        if (!data.CanLoadMore) {
                           messageView.hideLoadMoreBtn();
                        }
                   }
                });
            }
        };
        
    };

    messageBoard.domain.Message = message;

})($, messageBoard || {});


(function ($, messageBoard) {
    
    messageBoard.MessageView = messageBoard.MessageView || {};

    var messageView = messageBoard.MessageView;

    $.extend(messageView, {
        containerEl: null,
        setContainerElement: function (containerEl) {
            this.containerEl = containerEl;
        },

        logError: function (error, func) {
            console.log("messageBoard::" + func + "::" + error);
        },
        
        trendingMessStack: [],

        render: function (messages) {
            
            if (typeof this.containerEl == "undefined") {
                this.logError("Container Element cannot be null", "render");
                return;
            }
            else {
                var messageLen = messages.length;
                var phtml = "";
                for (var i = 0; i < messageLen; i++) {
                    
                    if ($("p.message-content").length < 1) {
                        
                        if (messages[i].Content && typeof messages[i].Content !== "undefined") {
                            phtml = " <p class='message-content clear-bottom-border'>" + messages[i].Content + "</p > \n";
                        }
                        else {
                            phtml = " <p class='message-content clear-bottom-border'>" + messages[i].content + "</p > \n";
                        }
                        $(phtml).appendTo($(this.containerEl));
                    }
                    else {
                        if (messages[i].Content && typeof messages[i].Content !== "undefined") {
                            phtml = " <p class='message-content'>" + messages[i].Content + "</p > \n";
                        }
                        else {
                            phtml = " <p class='message-content'>" + messages[i].content + "</p > \n";
                        }
                       
                        $(phtml).insertBefore($("p.message-content").first());
                    }
                }
            }
        },
        
        renderPrevious: function (messages) {

            if (typeof this.containerEl == "undefined") {
                this.logError("Container Element cannot be null", "render");
                return;
            }
            else {
                $("p.message-content").last().removeClass("clear-bottom-border");
                var messageLen = messages.length;
                var phtml = "";
                for (var i = 0; i < messageLen; i++) {
                    
                    if (i == messageLen - 1) {
                        if (messages[i].Content && typeof messages[i].Content !== "undefined") {
                            phtml = " <p class='message-content clear-bottom-border'>" + messages[i].Content + "</p > \n";
                        }
                        else {
                            phtml = " <p class='message-content clear-bottom-border'>" + messages[i].content + "</p > \n";
                        }
                    } else {
                        if (messages[i].Content && typeof messages[i].Content !== "undefined") {
                            phtml = " <p class='message-content'>" + messages[i].Content + "</p > \n";
                        }
                        else {
                            phtml = " <p class='message-content'>" + messages[i].content + "</p > \n";
                        }
                    }
                    $(phtml).insertAfter($("p.message-content").last());
                }
            }
        },
        
        renderTrending: function () {

            if (typeof this.containerEl == "undefined") {
                this.logError("Container Element cannot be null", "render");
                return;
            }
            else {
                var messageLen = this.messageStack.length;
                var phtml = "";
                for (var i = 0; i < messageLen; i++) {
                    phtml = " <p class='message-content'>" + this.messageStack[i].Content + "</p > \n";
                    $(phtml).insertBefore($("p.message-content").first());
                }
                this.messageStack = [];
            }
        },
        
        showUpdate: function (updateElem) {
            var messLen = this.trendingMessStack.length;
            var updMess;
            if (messLen > 1) {
                updMess = messLen + " new messages";
            }
            else {
                updMess = messLen + " new message";
            }
            
            $(updateElem).text(updMess).show();
            
            if (typeof messageBoard.Utils.changeTitleInterval !== "undefined") {
                clearInterval(messageBoard.Utils.changeTitleInterval);
            }
            messageBoard.Utils.changeTitleInterval = messageBoard.Utils.blinkTitleOnNewMessage(messageBoard.Utils.originalDomTitle, updMess, 2000);
        },
        
        hideLoadMoreBtn: function () {
            $("button.load-more-btn").hide();
        }
    });

})($, messageBoard || {});

(function () {
    $(document).ready(function () {
        var view = messageBoard.MessageView;
        view.setContainerElement("#mess-list");
        
        window.firstMessageIdOnFirstLoad = window.refreshStartId =  parseInt($("#firstMessageIdOnFirstLoad").val()),
        window.currentNumOfMessFetched = parseInt($("#currentNumOfMessFetched").val()),
        window.pageNo = parseInt($("#pageNo").val());
        
        $("button.save-btn").click(function (ev) {
            ev.preventDefault();
            
            var btn = this;
            var rtf = $("div.rtf");
            var message = new messageBoard.domain.Message($(rtf).text());
            
            $(btn).text("Posting...");
            $(btn).attr("disabled", "disabled");            


            var callback = function() {
                $(btn).text("Post");
                $(btn).removeAttr("disabled");
                $(rtf).text("Write a new message...");
                $(rtf).removeClass("onkeyPressed");
                //hide no message if shown
                if ($(".no-mess")) {
                    $(".no-mess").hide();
                }
            };

            try {
                message.save(callback, view, message);
            }
            catch (e) {
                alert(e.toString());
                $(btn).text("Post");
                $(btn).removeAttr("disabled");
                $(rtf).text("Write a new message...");
                $(rtf).removeClass("onkeyPressed");
            }
           
        });

        $("button.load-more-btn").click(function (ev) {
            ev.preventDefault();
            var btn = this;
            
            $(btn).text("Loading...");
            $(btn).attr("disabled", "disabled");
            var message = new messageBoard.domain.Message("");

            try {
                 message.loadPreviousMessages(view);
                
            } catch (e) {
                alert(e.toString());
            } 
           
            $(btn).text("Load More");
            $(btn).removeAttr("disabled");
           
        });

        $("button.upd-btn").click(function (ev) {
            ev.preventDefault();
            
            //hide no message if shown
            if ($(".no-mess")) {
                $(".no-mess").hide();
            }
                
            view.render(view.trendingMessStack);
            view.trendingMessStack = [];
            $(this).text("").hide();
            clearInterval(messageBoard.Utils.changeTitleInterval);
            $('title').text(messageBoard.Utils.revertDomTitle());
        });
        
        var refreshTrendingMessages = setInterval(function () {
            new messageBoard.domain.Message("").loadTrendingMessages(view, "button.upd-btn");
            
        }, 25000);


        $("div.rtf").focus();
        
        $("div.rtf").keydown(function () {
            var rtf = $(this);
            $(rtf).addClass("onkeyPressed");

            var textBeforeKeyDown = $(rtf).text();
            if ($.trim(textBeforeKeyDown.toLowerCase()) == "write a new message...") {
                $(rtf).text("");
            }
        });
        
        $("div.rtf").keyup(function () {
            var rtf = $(this);
            var textAfterKeyUp = $(rtf).text();
            if ($.trim(textAfterKeyUp) == "") {
                $(rtf).removeClass("onkeyPressed");
                $(rtf).text("Write a new message...");
            }
        });
    });
}).call(this);


(function ($, messageBoard) {

    messageBoard.Utils = messageBoard.Utils || {};

    var utils = messageBoard.Utils;

    $.extend(utils, {
        originalDomTitle: $('title').text(),
        
        revertDomTitle: function () {
            $('title').text(this.originalDomTitle);
        },
        
        changeTitleInterval: null,
        
        blinkTitleOnNewMessage: function (originalTitle, newTitle, blinkInterval) {
            var track = 1;
            var changeTitleInterval = setInterval(function () {
                if (track == 1)
                    $('title').text(newTitle);
                else {
                    $('title').text(originalTitle);
                }
                track = track * -1;
            }, blinkInterval);

            return changeTitleInterval;
        }
    });

})($, messageBoard || {});