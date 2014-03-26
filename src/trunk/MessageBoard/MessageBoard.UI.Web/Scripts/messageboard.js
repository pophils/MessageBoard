
var messageBoard = messageBoard || {};

(function ($, messageBoard) {
    messageBoard.domain = messageBoard.domain || {};

    var message = function (content) {
        return {
            content: content,
            datesaved: null,
            save: function () {
                if (typeof content == "string" && content.length > 0) {
                    $.post("/save-message", { Content: this.content }, function (data) {
                        if (!data.Success) {
                            throw new Error("An error occured saving message, please try again."); 
                        } else {
                            window.refreshStartId = data.CurrentRefreshStartId;
                        }
                    });
                } else {
                    throw new Error("Message content can't be empty");
                }
            },
            
            loadTrendingMessages: function (messageView) {
          
                $.getJSON("/get-trending-messages", { refreshStartId: window.refreshStartId }, function (data) {

                    var models = [];

                    if (data != null && data.Messages != null) {
                        for (var i = 0; i < data.Messages.length; i++) {
                            models.push(data.Messages[i]);
                        }

                        messageView.renderTrending(models);
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

    messageBoard.domain.message = message;

})($, messageBoard || {});


(function ($, messageBoard) {
    messageBoard.messageView = messageBoard.messageView || {};

    var messageView = messageBoard.messageView;

    $.extend(messageView, {
        containerEl: null,
        setContainerElement: function (containerEl) {
            this.containerEl = containerEl;
        },

        logError: function (error, func) {
            console.log("messageBoard::" + func + "::" + " error");
        },

        render: function (messages) {
            
            if (typeof this.containerEl == "undefined") {
                this.logError("Container Element cannot be null", "render");
                return;
            }
            else if (typeof messages == "undefined") {
                this.logError("No message to render", "render");
                return;
            }
            else {
                var messageLen = messages.length;
                var phtml = "";
                for (var i = 0; i < messageLen; i++) {
                    
                    if ($("p.message-content").length < 1) {
                        phtml = " <p class='message-content clear-bottom-border'>" + messages[i].content + "</p > \n";
                        $(phtml).appendTo($(this.containerEl));
                    }
                     else {
                        phtml = " <p class='message-content'>" + messages[i].content + "</p > \n";
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
            else if (typeof messages == "undefined") {
                this.logError("No message to render", "render");
                return;
            }
            else {
                
                $("p.message-content").last().removeClass("clear-bottom-border");
                var messageLen = messages.length;
                var phtml = "";
                for (var i = 0; i < messageLen; i++) {
                    
                    if(i == messageLen - 1) {
                        phtml = " <p class='message-content clear-bottom-border'>" + messages[i].Content + "</p > \n";
                    } else {
                        phtml = " <p class='message-content'>" + messages[i].Content + "</p > \n";
                    }

                    $(phtml).insertAfter($("p.message-content").last());
                }
            }
        },
        
        renderTrending: function (messages) {

            if (typeof this.containerEl == "undefined") {
                this.logError("Container Element cannot be null", "render");
                return;
            }
            else if (typeof messages == "undefined") {
                this.logError("No message to render", "render");
                return;
            }
            else {
                var messageLen = messages.length;
                var phtml = "";
                for (var i = 0; i < messageLen; i++) {
                    phtml = " <p class='message-content'>" + messages[i].Content + "</p > \n";
                    $(phtml).insertBefore($("p.message-content").first());
                }
            }
        },
        
        hideLoadMoreBtn: function () {
            $("button.load-more-btn").hide();
        }
    });

})($, messageBoard || {});


(function () {
    $(document).ready(function () {

        window.firstMessageIdOnFirstLoad = window.refreshStartId =  parseInt($("#firstMessageIdOnFirstLoad").val()),
            window.currentNumOfMessFetched = parseInt($("#currentNumOfMessFetched").val()), window.pageNo = parseInt($("#pageNo").val());
        
        $("button.save-btn").click(function (ev) {
            ev.preventDefault();
            var btn = this;
            var rtf = $("div.rtf");
            $(btn).text("Saving...");
            $(btn).attr("disabled", "disabled");
            var message = new messageBoard.domain.message($(rtf).text());

            try {
                message.save();
                $(".no-mess").hide();
                var view = messageBoard.messageView;
                view.setContainerElement("#mess-list");
                var messageArray = [];
                messageArray.push(message);
                view.render(messageArray);
            } catch (e) {

                alert(e.toString());
            } 
           
            $(btn).text("Save");
            $(btn).removeAttr("disabled");
            $(rtf).text("Write a new message");
        });

        $("button.load-more-btn").click(function (ev) {
            ev.preventDefault();
            var btn = this;
            
            $(btn).text("Loading...");
            $(btn).attr("disabled", "disabled");
            var message = new messageBoard.domain.message("");

            try {
                var view = messageBoard.messageView;
                message.loadPreviousMessages(view);
                
            } catch (e) {

                throw e;
                alert(e.toString());
            } 
           
            $(btn).text("Load More");
            $(btn).removeAttr("disabled");
           
        });

        var refreshTrendingMessages = setInterval(function () {
            
            var view = messageBoard.messageView;
            view.setContainerElement("#mess-list");
             new messageBoard.domain.message("").loadTrendingMessages(view);
            
        }, 15000);

        $("div.rtf").focus();
        
    });
}).call(this);