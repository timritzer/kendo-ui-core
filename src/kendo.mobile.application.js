function Application(options) {
    this.views = [];
    $.extend(this, options);
}

Application.prototype = {
    run: function(callback) {
        this.root = document.body;

        callback(this);
        this.show(0);

        window.scrollTo(0, 1);
    },

    addView: function(view) {
        this.views.push(view);
    },

    show: function(viewIndex, options) {
        if (this.currentView) {
            this.currentView.hide();
        }

        var view = this.views[viewIndex]; 
        
        view.show(this.root);

        this.currentView = view;
    }
};

