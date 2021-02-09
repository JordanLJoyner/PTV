#Source GUide: https://codeburst.io/this-is-how-easy-it-is-to-create-a-rest-api-8a25122ab1f3
#imports we need for RESTFUL APi
from flask import Flask
from flask_restful import Api, Resource, reqparse
from flask_cors import CORS
from flask_classy import FlaskView, route
import copy

app = Flask(__name__)
CORS(app)
api = Api(app)

videoSeries = ""
active_series = ""
up_next = ""
messageQueue = []
serverErrorCode = 404
serverSuccessCode = 200

class Series(Resource):
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("series_list")
        args = parser.parse_args()
        print(args["series_list"])
        global videoSeries
        videoSeries = args["series_list"]
        return "posted", serverSuccessCode

    def get(self):
        print("fetching series")
        return videoSeries, serverSuccessCode



class Schedule(Resource):
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("active_series")
        parser.add_argument("up_next")
        args = parser.parse_args()
        active_series = args["active_series"]
        up_next = args["up_next"]
        print(args)
        return "updated", serverSuccessCode

    def get(self):
        return videoSeries, serverSuccessCode

class Util(Resource):
    def get(self):
        return users, serverSuccessCode

class PTVMessageQueue(Resource):
    def get(self):
        messageQueueClone = copy.deepcopy(messageQueue)
        messageQueue.clear()
        return messageQueueClone, serverSuccessCode

    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("MessageType")
        args = parser.parse_args()
        message  = args["MessageType"]
        messageQueue.append(message)
        return "posted " + message, serverSuccessCode

class SkipShow(Resource):
    def post(self):
        skip_message = {"MessageType": "SKIP"}
        messageQueue.append(skip_message)
        return "skip request logged", serverSuccessCode

class VetoShow(Resource):
    def post(self):
        veto_message = {"MessageType": "VETO"}
        messageQueue.append(veto_message)
        return "veto request logged", serverSuccessCode

class Emote(Resource):
    def post(self):
        emote_message = {"MessageType": "EMOTE_WTF"}
        messageQueue.append(emote_message)
        return "WTF Emote request logged", serverSuccessCode

class Request(Resource):
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("ShowName")
        args = parser.parse_args()
        showName = args["ShowName"];
        print(showName)
        print(videoSeries)
        if showName in videoSeries:
            requestMessage = {"MessageType": "REQUEST", "Data": showName }
            messageQueue.append(requestMessage)
            return "Request for " + args["ShowName"] + " logged", serverSuccessCode;        
        else:
            return "Could not find a matching show for " + showName, serverErrorCode

mSongName = "No song registered"

class Song(Resource):        
    def get(self):
        return mSongName, serverSuccessCode
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("SongName")
        args = parser.parse_args()
        global mSongName
        mSongName = args["SongName"]
        return mSongName + " registered", serverSuccessCode
 
mShowName = "No show registered"
class Show(Resource):        
    def get(self):
        return mShowName, 200
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("ShowName")
        args = parser.parse_args()
        global mShowName
        mShowName = args["ShowName"]
        return mShowName + " registered", serverSuccessCode

mSchedule = {}
class Schedule(Resource):        
    def get(self):
        if(mSchedule == {}):
            return "Not set", serverErrorCode
        return mSchedule, serverSuccessCode
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("Schedule")
        args = parser.parse_args()
        global mSchedule
        print(args["Schedule"])
        mSchedule = args["Schedule"]
        return "scheduled registered", serverSuccessCode

mTime = ""
class Time(Resource):        
    def get(self):
        if mTime == "":
            return "Time not set", serverErrorCode
        return mTime, serverSuccessCode
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("TimeLeft")
        args = parser.parse_args()
        global mTime
        mTime = args["TimeLeft"]
        return mTime + " registered", serverSuccessCode

class Start(Resource):
    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("Shows")
        args = parser.parse_args()
        showNames = args["Shows"];
        print(showNames)
        print(videoSeries)
        requestMessage = {"MessageType": "START", "Data": showNames }
        messageQueue.append(requestMessage)
        return "Request to start with " + args["Shows"] + " logged", serverSuccessCode;

class Play(Resource):
    def post(self):
        emote_message = {"MessageType": "PLAY"}
        messageQueue.append(emote_message)
        return "Play request logged", serverSuccessCode

class Pause(Resource):
    def post(self):
        emote_message = {"MessageType": "PAUSE"}
        messageQueue.append(emote_message)
        return "Pause request logged", serverSuccessCode
        
#used to fix cors issue from html
#https://stackoverflow.com/questions/23741362/getting-cors-cross-origin-error-when-using-python-flask-restful-with-consum
@app.after_request
def after_request(response):
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type,Authorization')
    response.headers.add('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE')
    return response

#typically this would be in a database but we doing quick and dirty demos
users = [
    {
        "name":"Nicholas",
        "age":42,
        "occupation": "Network Engineer"
    },
    {
        "name":"Elvin",
        "age":32,
        "occupation": "Doctor"
    },
    {
        "name":"Jass",
        "age":22,
        "occupation": "Web Engineer"
    }
]

class User(Resource):

    #look through our users for the user, if we find it, return it, otherwise return 404
    def get(self,name):
        for user in users:
            if(name == user["name"]):
                return user, 200
        return "User not found", 404
    
    #
    def post(self, name):
        parser = reqparse.RequestParser()
        parser.add_argument("age")
        parser.add_argument("occupation")
        args = parser.parse_args()
        
        for user in users:
            if(name == user["name"]):
                return "User with name {} already exists".format(name), 400

        user = {
            "name": name,
            "age": args["age"],
            "occupation": args["occupation"]
        }
        print("yata2")
        users.append(user)
        return user, 201

    def put(self, name):
        parser = reqparse.RequestParser()
        parser.add_argument("age")
        parser.add_argument("occupation")
        args = parser.parse_args()

        for user in users:
            if(name == user["name"]):
                user["age"] = args["age"]
                user["occupation"] = args["occupation"]
                return user, 200
        
        user = {
            "name": name,
            "age": args["age"],
            "occupation": args["occupation"]
        }
        users.append(user)
        return user, 201

    def delete(self, name):
        global users
        users = [user for user in users if user["name"] != name]
        return "{} is deleted.".format(name), 200


api.add_resource(User, "/user/<string:name>")
api.add_resource(Util, "/Util/")
api.add_resource(Series,"/PTV/series/")
api.add_resource(PTVMessageQueue,"/PTVMessageQueue/")
api.add_resource(SkipShow,"/PTV/SkipShow/")
api.add_resource(VetoShow,"/PTV/Veto/")
api.add_resource(Song,"/PTV/song/")
api.add_resource(Show,"/PTV/show/")
api.add_resource(Schedule,"/PTV/schedule/")
api.add_resource(Emote,"/PTV/emote/")
api.add_resource(Request,"/PTV/request/")
api.add_resource(Time,"/PTV/time/")
api.add_resource(Start,"/PTV/start/")
api.add_resource(Play,"/PTV/play/")
api.add_resource(Pause,"/PTV/pause/")


if __name__ == '__main__':
    app.run(host='0.0.0.0')
else:
    app.run(debug=True)