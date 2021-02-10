#Source GUide: https://codeburst.io/this-is-how-easy-it-is-to-create-a-rest-api-8a25122ab1f3
#imports we need for RESTFUL APi
from flask import Flask
from flask_restful import Api, Resource, reqparse
from flask_cors import CORS
from flask_classy import FlaskView, route
import copy
import json

app = Flask(__name__)
CORS(app)
api = Api(app)

serverErrorCode = 404
serverSuccessCode = 200
STATUS_FIELD = "status"
STATUS_AVAILABLE = "available"
STATUS_BUSY = "busy"
STATUS_PLAYING = "playing"

videoSeries = ""
messageQueue = []
mTime = ""

mRoomId = 1
mRooms = [
	#Example room
    {
        "name": "JTown",
        "theater_name": "jordan's computer",
        "url": "https://content.jwplatform.com/manifests/Y5UQq0fG.m3u8",
        "id": 0,
        "viewers": 0,
        "current_show": "Batman Beyond",
        "series": "Batman Beyond,Jackie Chan Adventures,Medabots",
        "status": "available",
        "song_name": "No song registered",
    	"firebaseid": "firebase1",
    	"schedule": {}
    }
]

class Series(Resource):
    def post(self, id):
        parser = reqparse.RequestParser()
        parser.add_argument("series_list")
        args = parser.parse_args()
        print(args["series_list"])
        room = getRoomForId(id)
        room["series"] = args["series_list"]
        return "posted", serverSuccessCode

    def get(self, id):
        print("fetching series")
        room = getRoomForId(id)
        return room["series"], serverSuccessCode

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
        parser.add_argument("showName")
        args = parser.parse_args()
        showName = args["showName"];
        print(showName)
        if showName in videoSeries:
            requestMessage = {"MessageType": "REQUEST", "Data": showName }
            messageQueue.append(requestMessage)
            return "Request for " + args["ShowName"] + " logged", serverSuccessCode;        
        else:
            return "Could not find a matching show for " + showName, serverErrorCode

class Song(Resource):        
    def get(self,id):
        room = getRoomForId(id)
        return room["song_name"], serverSuccessCode
    def post(self,id):
        parser = reqparse.RequestParser()
        parser.add_argument("SongName")
        args = parser.parse_args()
        room = getRoomForId(id)
        print(room)
        room["song_name"] = args["SongName"]
        return room["song_name"] + " registered", serverSuccessCode
 
class Show(Resource):        
    def get(self, id):
        room = getRoomForId(id)
        return room["current_show"], 200
    def post(self, id):
        parser = reqparse.RequestParser()
        parser.add_argument("ShowName")
        args = parser.parse_args()
        room = getRoomForId(id)
        print(room)
        room["current_show"] = args["ShowName"]
        return room["current_show"] + " registered", serverSuccessCode

class Schedule(Resource):        
    def get(self, id):
        room = getRoomForId(id)
        if(room['schedule'] == {} or room['schedule'] == None):
            return "Not set", serverErrorCode
        return room['schedule'], serverSuccessCode
    def post(self, id):
        parser = reqparse.RequestParser()
        parser.add_argument("Schedule")
        args = parser.parse_args()
        print(args["Schedule"])
        room = getRoomForId(id)
        room['schedule'] = args["Schedule"]
        return "schedule registered", serverSuccessCode

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

class Rooms(Resource):
    def get(self):
        global mRooms
        return mRooms, serverSuccessCode   

    def post(self):
        parser = reqparse.RequestParser()
        parser.add_argument("room")
        args = parser.parse_args()
        print(args)
        print("\n\n")
        newRoom = args["room"]
        if isinstance(newRoom,str):
            newRoom = json.loads(newRoom)
        print(newRoom)
        newRoom[STATUS_FIELD] = STATUS_AVAILABLE
        mRooms.append(newRoom)
        return "Added new room", serverSuccessCode
    
    def delete(self):
        global mRooms
        mRooms.clear()
        return "Cleared Rooms", serverSuccessCode

def getRoomForId(id):
    global mRooms
    for room in mRooms:
        roomJson = room
        if roomJson["id"] == id:
            if isinstance(roomJson,str):
                room = json.loads(roomJson)
            return room 
    return None

class Room(Resource):
    def get(self, id):
        return getRoomForId(id), serverSuccessCode

    def delete(self, id):
        roomToRemove = getRoomForId(id)
        print("ID to remove: " + str(id))
        if roomToRemove != None:
	        print("Removing Room: " + str(id))
	        global mRooms
	        mRooms.remove(roomToRemove)

class RoomId(Resource):
    def get(self):
        global mRoomId
        value = mRoomId
        mRoomId = mRoomId + 1
        return  value, serverSuccessCode;  

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

class Host(Resource):
    def post(self, id):
        room = getRoomForId(id)
        parser = reqparse.RequestParser()
        parser.add_argument("name")
        parser.add_argument("firebaseid")
        parser.add_argument("shows")
        args = parser.parse_args()
        room["name"] = args["name"]
        room["firebaseid"] = args["firebaseid"]
        room[STATUS_FIELD] = STATUS_PLAYING
        showNames = args["shows"];
        print("Requested Host with \n"+showNames)
        requestMessage = {"MessageType": "START", "Data": showNames }
        messageQueue.append(requestMessage)
        return  "Hosting started", serverSuccessCode;  

class ChangeRoomStatus(Resource):
    def put(self, id):
        room = getRoomForId(id)
        parser = reqparse.RequestParser()
        parser.add_argument(STATUS_FIELD)
        args = parser.parse_args()
        newStatus = args[STATUS_FIELD]
        print(newStatus)
        print(room)
        room[STATUS_FIELD] = newStatus
        return "Updated " + str(room), serverSuccessCode

class ChangeStreamURL(Resource):
    def put(self, id):
        room = getRoomForId(id)
        parser = reqparse.RequestParser()
        parser.add_argument("url")
        args = parser.parse_args()
        newUrl = args["url"]
        room["url"] = newUrl
        return str(id) + " now has stream url " + str(newUrl), serverSuccessCode        
#left here for reference
#api.add_resource(User, "/user/<string:name>")
#api.add_resource(Util, "/Util/")

prefix = "/PTV"
roomPrefix = "/room/<int:id>"
totalRoomPrefix = prefix + roomPrefix

api.add_resource(Series,totalRoomPrefix+"/series/")

api.add_resource(PTVMessageQueue,"/PTVMessageQueue/")

api.add_resource(SkipShow,prefix+"/SkipShow/")
api.add_resource(VetoShow,prefix+"/Veto/")
api.add_resource(Song,totalRoomPrefix+"/song/")
api.add_resource(Show,totalRoomPrefix+"/show/")

api.add_resource(Schedule,totalRoomPrefix+"/schedule/")
api.add_resource(Emote,prefix+"/emote/")
api.add_resource(Request,prefix+"/request/")
api.add_resource(Time,prefix+"/time/")
api.add_resource(Play,prefix+"/play/")
api.add_resource(Pause,prefix+"/pause/")

api.add_resource(Rooms,prefix+"/rooms/")

#REST api to mess with individual rooms
api.add_resource(Room, prefix+"/room/<int:id>")

#Used by the theater software to establish a new room
api.add_resource(RoomId,prefix+"/rooms/newid")

#Used by the hackweek app software to take over a new room
api.add_resource(Host,totalRoomPrefix+"/host")
api.add_resource(ChangeRoomStatus,totalRoomPrefix+"/status")
api.add_resource(ChangeStreamURL,totalRoomPrefix+"/url")

if __name__ == '__main__':
    app.run(host='0.0.0.0')
else:
    app.run(debug=True)