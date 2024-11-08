#服务端协议导出脚本
#约定：
#proto文件所在：~/tf/proto/
#服务端导出pbgo文件在：~/tf_all/pb/
protoc --proto_path=. --go_out=plugins=kite:../../cy_all role.proto
protoc --proto_path=. --go_out=plugins=kite:../../cy_all comm.proto
protoc --proto_path=. --go_out=plugins=kite:../../cy_all logic.proto
protoc --proto_path=. --go_out=plugins=kite:../../cy_all Ugate.proto